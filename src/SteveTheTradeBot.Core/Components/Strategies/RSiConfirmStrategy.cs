using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class RSiConfirmStrategy : BaseStrategy
    {
        public const string Desc = nameof(RSiConfirmStrategy);
        public override string Name => Desc;

        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _buySignal;
        private readonly int _quotesToCheckRsi;
        private readonly int _positiveTrendOverQuotes;
        private readonly ICloseSignal _closeSignal;


        public RSiConfirmStrategy() 
        {
            _closeSignal = new RaiseManualStopLossCloseSignal(0.96m, 1.05m);
            _buySignal = 30;
            _quotesToCheckRsi = 10;
            _positiveTrendOverQuotes = 3;
        }

        public override async Task DataReceived(StrategyContext data)
        {
            var currentTrade = data.Quotes.Last();
            var activeTrade = data.ActiveTrade();

            if (activeTrade == null)
            {
                var tradeQuotes = data.Quotes.TakeLast(_quotesToCheckRsi + _positiveTrendOverQuotes).Take(_quotesToCheckRsi).ToArray();
                var minRsi = Signals.Rsi.MinRsi(tradeQuotes);
                var hasBuySignal = Signals.Rsi.HasBuySignal(tradeQuotes, _buySignal);
                var isPositiveTrend = Signals.IsPositiveTrend(data.Quotes.TakeLast(_positiveTrendOverQuotes));
                
                var isOutOfCoolDownPeriod = Signals.IsOutOfCoolDownPeriod(data);
                    
                if (hasBuySignal && isPositiveTrend && isOutOfCoolDownPeriod)
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
                    data.StrategyInstance.Status =
                        $"Waiting to buy ![wait for min rsi {minRsi} <= {_buySignal} in last {_quotesToCheckRsi}] [isPositiveTrend {isPositiveTrend} [{data.Quotes.TakeLast(_positiveTrendOverQuotes).Select(x=>x.Close.ToString(CultureInfo.InvariantCulture)).StringJoin()}]]";
                }
            }
            else
            {
                await _closeSignal.DetectClose(data, currentTrade, activeTrade,this);
            }
        }
    }
}