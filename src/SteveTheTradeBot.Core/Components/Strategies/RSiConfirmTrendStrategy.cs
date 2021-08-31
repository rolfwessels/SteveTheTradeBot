using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Serilog;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.BackTesting;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class RSiConfirmTrendStrategy : BaseStrategy
    {
        public const string Desc = nameof(RSiConfirmTrendStrategy);
        public override string Name => Desc;

        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _buySignal;
        private readonly int _quotesToCheckRsi;
        private readonly int _positiveTrendOverQuotes;
        private readonly ICloseSignal _closeSignal;


        public RSiConfirmTrendStrategy()
        {
            //_closeSignal = new RaiseStopLossCloseSignalDynamic(0.02m, 0.01m, 0.10m);
            _closeSignal = new RaiseStopLossCloseSignalDynamic(0.04m, 0.01m, 0.05m);

            _buySignal = 30;
            _quotesToCheckRsi = 20; 
            _positiveTrendOverQuotes = 3;
        }

        public override async Task DataReceived(StrategyContext data)
        {
            var currentTrade = data.ByMinute.Last();
            var activeTrade = data.ActiveTrade();

           
            if (activeTrade == null)
            {
                var tradeQuotes = data.ByMinute.TakeLast(_quotesToCheckRsi + _positiveTrendOverQuotes).Take(_quotesToCheckRsi).ToArray();
                var minRsi = Signals.Rsi.MinRsi(tradeQuotes);
                var hasBuySignal = Signals.Rsi.HasBuySignal(tradeQuotes, _buySignal);
                
                var isUpTrend = Signals.Ema.IsUpTrend(currentTrade);
                var isOutOfCoolDownPeriod = Signals.IsOutOfCoolDownPeriod(data);
                if (hasBuySignal && isUpTrend && isOutOfCoolDownPeriod)
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to buy at {currentTrade.Close} Rsi:{hasBuySignal}");
                    var strategyTrade = await Buy(data, data.StrategyInstance.QuoteAmount);
                    var resetStops = await _closeSignal.Initialize(data, currentTrade.Close, this);
                    data.StrategyInstance.Status =
                        $"Bought! [{strategyTrade.BuyPrice} and set stop loss at {resetStops}]";
                }
                else
                {
                    data.StrategyInstance.Status = "Waiting to buy!";
                }
            }
            else
            {
                await _closeSignal.DetectClose(data, currentTrade, activeTrade, this);
            }
        }
    }
}