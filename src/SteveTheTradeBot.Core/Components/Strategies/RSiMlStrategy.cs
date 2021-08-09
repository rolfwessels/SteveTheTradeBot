﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.ML.Model;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class RSiMlStrategy : RaiseStopLossOutStrategyBase
    {
        public const string Desc = nameof(RSiMlStrategy);

        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _buySignal;
        private readonly int _quotesToCheckRsi;
        private readonly ConsumeModel _consumeModel;


        public RSiMlStrategy() : base(0.96m, 1.05m)
        {
            _buySignal = 30;
            _quotesToCheckRsi = 10;
            _consumeModel = new ConsumeModel(@"C:\temp\MLModel.zip");
        }

        public override async Task DataReceived(StrategyContext data)
        {
            var currentTrade = data.ByMinute.Last();
            var activeTrade = data.ActiveTrade();
            
            var hasRecentlyHitOverSold = data.ByMinute.TakeLast(_quotesToCheckRsi).Take(_quotesToCheckRsi).Min(x => x.Metric.GetOrDefault("rsi14"));
            var predictedGrowth = _consumeModel.Predict(currentTrade.ToModelInput()).Score;
            if (activeTrade == null)
            {

                if (hasRecentlyHitOverSold <= _buySignal && predictedGrowth > 1)
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
                        $"Waiting to buy ![wait for min rsi {hasRecentlyHitOverSold} <= {_buySignal} in last {_quotesToCheckRsi}] [isPositiveTrend {predictedGrowth} > 1]";
                }
            }
            else
            {
                await RaiseStopLoss(data, currentTrade, activeTrade);
            }
        }

        private bool IsPositiveTrend(IEnumerable<TradeQuote> values)
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