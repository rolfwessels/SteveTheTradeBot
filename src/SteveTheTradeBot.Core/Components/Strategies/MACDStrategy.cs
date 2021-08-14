using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class MacdStrategy : BaseStrategy
    {
        public const string Desc = nameof(MacdStrategy);
        public override string Name => Desc;

        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICloseSignal _closeSignal;


        public MacdStrategy()
        {
            _closeSignal = new RaiseStopLossOutCloseSignal(0.98m, 1.02m);
        }

        public override async Task DataReceived(StrategyContext data)
        {
            var currentTrade = data.ByMinute.Last();
            var activeTrade = data.ActiveTrade();

           if (activeTrade == null) 
            {
                var tradeQuotes = data.ByMinute.TakeLast(4).ToArray();
                var crossed = Signals.Macd.GetCrossedMacdOverSignal(tradeQuotes);
                var isCrossedBelowZero = Signals.Macd.IsCrossedBelowZero(crossed);
                var isUpTrend = crossed.Any() && Signals.Ema.IsUpTrend(tradeQuotes.Last());
                var shouldBuy = crossed.Any() && isCrossedBelowZero && isUpTrend;
                if (shouldBuy)
                {
                    var strategyTrade = await Buy(data, data.StrategyInstance.QuoteAmount);
                    var resetStops = await _closeSignal.Initialize(data, currentTrade.Close,this);
                    data.StrategyInstance.Status = $"{strategyTrade.ToString(data.StrategyInstance)}. Set stop loss at {resetStops}]";
                    _log.Information($"{data.StrategyInstance.Name} {data.StrategyInstance.Status}");
                }
                else
                {
                    data.StrategyInstance.Status = $"Waiting for crossed ({crossed.Any()}) and isCrossedBelowZero ({isCrossedBelowZero}) and isUpTrend {isUpTrend}";
                }
            }
            else
            {
                await _closeSignal.DetectClose(data, currentTrade, activeTrade,this);
            }
        }

        private static void PrintDebug(TradeQuote currentTrade, List<TradeQuote> crossed)
        {
            Console.Out.WriteLine(currentTrade.Date.ToString(CultureInfo.InvariantCulture));
            Console.Out.WriteLine(crossed
                .Select(x => new
                {
                    MacdValue = x.Metric.GetOrDefault(Signals.MacdValue),

                    MacdSignal = x.Metric.GetOrDefault(Signals.MacdSignal),
                    Ema = x.Metric.GetOrDefault(Signals.Ema200),
                    x.Close
                }).ToTable());
        }
    }
}