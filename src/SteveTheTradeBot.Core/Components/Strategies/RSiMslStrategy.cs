using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Hangfire.Logging;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class RSiMslStrategy : RaiseStopLossOutStrategyBase
    {
        public const string Desc = "RSiMslStrategy";

        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _buySignal;
        private readonly decimal _buy200rocsma;


        public RSiMslStrategy() : base(0.96m, 1.05m)
        {
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
                    var strategyTrade = await Buy(data, data.StrategyInstance.QuoteAmount);
                    var resetStops = await ResetStops(data, currentTrade.Close);
                    data.StrategyInstance.Status =
                        $"Bought! [{strategyTrade.BuyPrice} and set stop loss at {resetStops}]";
                }
                else
                {
                    data.StrategyInstance.Status =
                        $"Waiting to buy ![{rsiResults} < {_buySignal}] [{roc200sma} > {_buy200rocsma}]";
                }
            }
            else
            {
                await FollowClosingStrategy(data, currentTrade, activeTrade);
            }
        }


        public override string Name => Desc;

       
    }
}