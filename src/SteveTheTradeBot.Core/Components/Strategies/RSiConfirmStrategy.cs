using System;
using System.Collections.Generic;
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
    public class RSiConfirmStrategy : RaiseStopLossOutStrategyBase
    {
        public const string Desc = nameof(RSiConfirmStrategy);

        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _buySignal;
        private readonly decimal _buy200rocsma;
        private int _quotesToCheckRsi;


        public RSiConfirmStrategy() : base(0.96m, 1.05m)
        {
            _buySignal = 30;
            _buy200rocsma = 0.5m;
            _quotesToCheckRsi = 10;
        }

        public override async Task DataReceived(StrategyContext data)
        {
            var currentTrade = data.ByMinute.Last();
            var activeTrade = data.ActiveTrade();
            
            var roc200sma = currentTrade.Metric.GetOrDefault("roc200-sma");
            var hasRecentlyHitOverSold = data.ByMinute.TakeLast(_quotesToCheckRsi).Min(x => x.Metric.GetOrDefault("rsi14"));
            var isPositiveTrend = IsPositiveTrend(data.ByMinute.TakeLast(3));
            if (activeTrade == null)
            {

                if (hasRecentlyHitOverSold <= _buySignal && isPositiveTrend && (roc200sma.HasValue && roc200sma.Value > _buy200rocsma))
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to buy at {currentTrade.Close} Rsi:{hasRecentlyHitOverSold} Rsi:{roc200sma.Value}");
                    var strategyTrade = await Buy(data, data.StrategyInstance.QuoteAmount);
                    ResetStops(currentTrade, data);
                    data.StrategyInstance.Status =
                        $"Bought! [{strategyTrade.BuyPrice} and set stop loss at {StopLoss(data)}]";
                }
                else
                {
                    data.StrategyInstance.Status =
                        $"Waiting to buy ![wait for min rsi {hasRecentlyHitOverSold} <= {_buySignal} in last {_quotesToCheckRsi}] [{roc200sma} > {_buy200rocsma}]";
                }
            }
            else
            {
                await RaiseStopLoss(data, currentTrade, activeTrade);
            }
        }

        private bool IsPositiveTrend(IEnumerable<TradeFeedCandle> values)
        {
            decimal lastValue = -1;
            foreach (var value in values)
            {
                if (lastValue != -1 && value.Close <= lastValue)
                {
                    return false;
                }

                lastValue = value.Close;
                
            }

            return true;
        }



        public override string Name => Desc;

       
    }
}