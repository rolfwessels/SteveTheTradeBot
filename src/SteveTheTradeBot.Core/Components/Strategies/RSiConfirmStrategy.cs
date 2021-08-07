﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Bumbershoot.Utilities.Helpers;
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
        private readonly int _quotesToCheckRsi;
        private readonly int _positiveTrendOverQuotes;


        public RSiConfirmStrategy() : base(0.96m, 1.05m)
        {
            _buySignal = 30;
            _quotesToCheckRsi = 10;
            _positiveTrendOverQuotes = 3;
        }

        public override async Task DataReceived(StrategyContext data)
        {
            var currentTrade = data.ByMinute.Last();
            var activeTrade = data.ActiveTrade();
            
            var hasRecentlyHitOverSold = data.ByMinute.TakeLast(_quotesToCheckRsi).Min(x => x.Metric.GetOrDefault("rsi14"));
            
            var isPositiveTrend = IsPositiveTrend(data.ByMinute.TakeLast(_positiveTrendOverQuotes));
            if (activeTrade == null)
            {

                if (hasRecentlyHitOverSold <= _buySignal && isPositiveTrend)
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to buy at {currentTrade.Close} Rsi:{hasRecentlyHitOverSold}");
                    var strategyTrade = await Buy(data, data.StrategyInstance.QuoteAmount);
                    ResetStops(currentTrade, data);
                    data.StrategyInstance.Status =
                        $"Bought! [{strategyTrade.BuyPrice} and set stop loss at {StopLoss(data)}]";
                }
                else
                {
                    data.StrategyInstance.Status =
                        $"Waiting to buy ![wait for min rsi {hasRecentlyHitOverSold} <= {_buySignal} in last {_quotesToCheckRsi}] [isPositiveTrend {isPositiveTrend} [{data.ByMinute.TakeLast(_positiveTrendOverQuotes).Select(x=>x.Close.ToString(CultureInfo.InvariantCulture)).StringJoin()}]]";
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