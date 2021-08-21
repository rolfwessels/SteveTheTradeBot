using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Hangfire.Dashboard;
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

        public RaiseStopLossCloseSignalDynamic(decimal maxRiskPercent, decimal updateAtIntervalPercent, decimal maxRange) : base(1- maxRiskPercent,1+ updateAtIntervalPercent)
        {
            _maxRiskPercent = maxRiskPercent;
            _updateAtIntervalPercent = updateAtIntervalPercent;
            _maxRange = maxRange;
        }

        #region Overrides of RaiseStopLossCloseSignal

        public override async Task UpdateStopLoss(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade, BaseStrategy strategy)
        {

            await data.Set(StrategyProperty.UpdateStopLossAt, currentTrade.Close * (1+ _updateAtIntervalPercent));
            
            var newStopLoss = currentTrade.Close * (1- _maxRiskPercent);
            var profit = TradeUtils.MovementPercent(currentTrade.Close, activeTrade.BuyPrice)/100;
           
            if (profit > (_maxRiskPercent*1.5m))
            {
                var profit1 = (profit * 0.5m);
                newStopLoss = currentTrade.Close * (1 - Math.Min(profit1,_maxRange));
            }

            await SetStopLossAndMessages(data, strategy, newStopLoss, activeTrade.BuyPrice);
        }

        #endregion
    }
}