using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class RaiseStopLossCloseSignalDynamic : RaiseStopLossCloseSignal
    {
        private readonly decimal _maxRiskPercent;
        private readonly decimal _updateAtIntervalPercent;
        private readonly decimal _maxRange;
        private readonly decimal _initialTakeProfit;

        public RaiseStopLossCloseSignalDynamic(decimal maxRiskPercent = 0.02m, decimal updateAtIntervalPercent = 0.01m, decimal maxRange = 0.05m) 
            : base(1- maxRiskPercent,1+ updateAtIntervalPercent)
        {
            _maxRiskPercent = maxRiskPercent;
            _updateAtIntervalPercent = updateAtIntervalPercent;
            _maxRange = maxRange;
            _initialTakeProfit = (_maxRiskPercent * 1.5m);
        }

        #region Overrides of RaiseStopLossCloseSignal

        public override async Task UpdateStopLoss(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade, BaseStrategy strategy)
        {

            await data.Set(StrategyProperty.UpdateStopLossAt, currentTrade.Close * (1+ _updateAtIntervalPercent));
            var newStopLoss = currentTrade.Close * (1- _maxRiskPercent);
            var profit = TradeUtils.MovementPercent(newStopLoss, activeTrade.BuyPrice)/100;
            
            if (profit > _initialTakeProfit)
            {
                var cutFromProfit = (profit * 0.8m);
                var min = Math.Min(cutFromProfit,_maxRange);
                newStopLoss = currentTrade.Close * (1 - min);
            }

            await SetStopLossAndMessages(data, strategy, newStopLoss, activeTrade.BuyPrice);
        }

        #endregion
    }
}