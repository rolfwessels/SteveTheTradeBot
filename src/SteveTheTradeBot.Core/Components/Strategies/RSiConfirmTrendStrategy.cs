using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
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
            _closeSignal = new RaiseStopLossCloseSignalDynamic(0.04m);
            _buySignal = 30;
            _quotesToCheckRsi = 20; 
            _positiveTrendOverQuotes = 3;
        }

        public override async Task DataReceived(StrategyContext data)
        {
            var currentTrade = data.Quotes.Last();
            var activeTrade = data.ActiveTrade();

           
            if (activeTrade == null)
            {
                var tradeQuotes = data.Quotes.TakeLast(_quotesToCheckRsi + _positiveTrendOverQuotes).Take(_quotesToCheckRsi).ToArray();
                var hasBuySignal = Signals.Rsi.HasBuySignal(tradeQuotes, _buySignal);
                var isUpTrend = Signals.Ema.IsUpTrend(currentTrade);
                var isOutOfCoolDownPeriod = Signals.IsOutOfCoolDownPeriod(data);
                var isBullishMarket30 = Signals.IsBullishMarket(data,overDays:30);

                if (hasBuySignal && isUpTrend && isOutOfCoolDownPeriod && isBullishMarket30 )
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