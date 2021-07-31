using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Bumbershoot.Utilities.Helpers;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class OutClassName
    {
        public OutClassName(decimal? returnValue)
        {
            ReturnValue = returnValue;
        }

        public decimal? ReturnValue { get; private set; }
    }

    public class RSiMslStrategy : BaseStrategy
    {
        public const string Desc = "RSiMslStrategy";

        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _buySignal;
        private readonly decimal _initialStopRisk;
        private readonly decimal _buy200rocsma;
        private readonly decimal _moveProfitPercent;


        public RSiMslStrategy()
        {
            _initialStopRisk = 0.96m;
            _moveProfitPercent = 1.05m;
            _buySignal = 30;
            _buy200rocsma = 0.5m;
        }

        public override async Task DataReceived(StrategyContext data)
        {
            var currentTrade = data.ByMinute.Last();
            var activeTrade = data.ActiveTrade();
            var rsiResults = currentTrade.Metric.GetOrDefault("rsi14");
            var roc200sma = currentTrade.Metric.GetOrDefault("roc200-sma");
            if (activeTrade == null)
            {
                if (rsiResults < _buySignal && (roc200sma.HasValue && roc200sma.Value > _buy200rocsma))
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to buy at {currentTrade.Close} Rsi:{rsiResults} Rsi:{roc200sma.Value}");
                    await Buy(data, data.StrategyInstance.BaseAmount);
                    ResetStops(currentTrade, data);
                }
            }
            else
            {
                if (currentTrade.Close > MoveProfit(data).Dump(""))
                {
                    ResetStops(currentTrade, data);
                }

                if (currentTrade.Close <= StopLoss(data))
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to sell at {currentTrade.Close} - {activeTrade.BuyPrice} = {currentTrade.Close - activeTrade.BuyPrice} Rsi:{rsiResults}");

                    await Sell(data, activeTrade);
                }
            }
        }

        private decimal? MoveProfit(StrategyContext data, decimal? setValue = null)
        {
            var key = "MoveProfit";
            if (setValue == null)
            {
                var moveProfitPercent = 551258;
                return data.Get(key, moveProfitPercent).Result;
            }
            data.Set(key, setValue.Value).Wait();
            return setValue;
        }

        private decimal? StopLoss(StrategyContext data, decimal? setValue = null)
        {
            var key = "StopLoss";
            if (setValue == null)
            {
                var moveProfitPercent = data.LatestQuote().Close * _initialStopRisk;
                return data.Get(key, moveProfitPercent).Result;
            }
            data.Set(key, setValue.Value).Wait();
            return setValue;
        }


        private void ResetStops(TradeFeedCandle currentTrade, StrategyContext data)
        {
            StopLoss(data, currentTrade.Close * _initialStopRisk);
            MoveProfit(data, currentTrade.Close * _moveProfitPercent);
        }

       

        public override string Name => Desc;

       
    }
}