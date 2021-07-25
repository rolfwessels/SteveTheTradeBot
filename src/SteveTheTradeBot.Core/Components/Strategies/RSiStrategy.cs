using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class RSiStrategy : BaseStrategy
    {
        public const string Desc = "SimpleRsi";

        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly int _buySignal;
        private readonly decimal _initialStopRisk;
        private decimal? _setStopLoss;
        private readonly decimal _buy200rocsma;
        private readonly decimal _initialTakeProfit;
        private decimal _setTakeProfit;
        private readonly decimal _moveProfitPercent;
        private decimal _setMoveProfit;

        public RSiStrategy() : base()
        {
            _initialStopRisk = 0.96m;
            _initialTakeProfit = 1.10m;
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
                    ResetStops(currentTrade);
                }
            }
            else
            {
                //if ( (rsiResults > _sellSignal && activeTrade.BuyPrice < currentTrade.Close) || currentTrade.Close <= _setStopLoss)
                if (currentTrade.Close > _setMoveProfit)
                {
                    ResetStops(currentTrade);
                }

                if ( currentTrade.Close <= _setStopLoss || currentTrade.Close >= _setTakeProfit)
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to sell at {currentTrade.Close} - {activeTrade.BuyPrice} = {currentTrade.Close - activeTrade.BuyPrice} Rsi:{rsiResults}");

                    await Sell(data, activeTrade);
                    _setStopLoss = null;
                }
            }
        }

        private void ResetStops(TradeFeedCandle currentTrade)
        {
            _setStopLoss = currentTrade.Close * _initialStopRisk;
            _setTakeProfit = currentTrade.Close * _initialTakeProfit;
            _setMoveProfit = currentTrade.Close * _moveProfitPercent;
        }

  

        public override string Name => Desc;
      
    }
}
