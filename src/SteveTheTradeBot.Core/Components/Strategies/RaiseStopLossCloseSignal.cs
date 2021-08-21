using System;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.Slack;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class RaiseStopLossCloseSignal : ICloseSignal
    {
        private readonly decimal _initialStopRisk;
        private readonly decimal _moveProfitPercent;

        public RaiseStopLossCloseSignal(decimal initialStopRisk = 0.96m, decimal moveProfitPercent = 1.05m)
        {
            _initialStopRisk = initialStopRisk;
            _moveProfitPercent = moveProfitPercent;
        }

        #region Implementation of ICloseSignal

        public async Task<decimal> Initialize(StrategyContext data, decimal boughtAtPrice, BaseStrategy strategy)
        {
            var lossAmount = boughtAtPrice * _initialStopRisk;
            await data.Set(StrategyProperty.UpdateStopLossAt, boughtAtPrice * _moveProfitPercent);
            await SetStopLossAndMessages(data, strategy, lossAmount, boughtAtPrice);
            return lossAmount;
        }

        protected virtual async Task SetTheStopLoss(StrategyContext data, BaseStrategy strategy, decimal lossAmount)
        {
            await strategy.SetStopLoss(data, lossAmount);
        }

        public virtual async Task DetectClose(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade,
            BaseStrategy strategy)
        {
            var updateStopLossAt = await GetUpdateStopLossAt(activeTrade, data);
            if (currentTrade.Close >= updateStopLossAt)
            {
                await UpdateStopLoss(data, currentTrade, activeTrade, strategy);
            }
            else
            {
                data.StrategyInstance.Status =
                    $"Waiting for price above {updateStopLossAt} or stop loss {await GetStopLoss(activeTrade, data)}";
            }
        }

        public virtual async Task UpdateStopLoss(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade,
            BaseStrategy strategy)
        {
            await data.Set(StrategyProperty.UpdateStopLossAt, currentTrade.Close * _moveProfitPercent);
            var newStopLoss = currentTrade.Close * _initialStopRisk;
            await SetStopLossAndMessages(data, strategy, newStopLoss, activeTrade.BuyPrice);
        }

        protected async Task SetStopLossAndMessages(StrategyContext data, BaseStrategy strategy,
            decimal newStopLoss, decimal buyPrice)
        {
            await data.Set(StrategyProperty.StopLoss, newStopLoss);
            await SetTheStopLoss(data, strategy, newStopLoss);
            var movementPercent = TradeUtils.MovementPercent(newStopLoss, buyPrice);
            data.StrategyInstance.Status = movementPercent > 0
                ? $"Update stop loss to {newStopLoss} that means guaranteed profit of {movementPercent}%"
                : $"Set stop loss to {newStopLoss} that means risk of {Math.Abs(movementPercent)}%";
            var icon = movementPercent>0? ":chart_with_upwards_trend:":"";

           // data.StrategyInstance.Status.Dump("");
            await data.Messenger.Send(PostSlackMessage.From(
                $"{data.StrategyInstance.Name} {data.StrategyInstance.Status} {icon}"));
        }

        protected static async Task<decimal> GetStopLoss(StrategyTrade activeTrade, StrategyContext data)
        {
            return await data.Get(StrategyProperty.StopLoss, activeTrade.GetValidStopLoss()?.OrderPrice??0);
        }

        #endregion


        #region Private Methods

        protected async Task<decimal> GetUpdateStopLossAt(StrategyTrade activeTrade, StrategyContext data)
        {
            var movePercent = await data.Get(StrategyProperty.UpdateStopLossAt, 0);
            if (movePercent != 0) return movePercent;
            var validStopLoss = activeTrade.GetValidStopLoss();
            if (validStopLoss == null) return activeTrade.BuyPrice;
            var moveProfitPercent = validStopLoss.OrderPrice * (_moveProfitPercent + (1 - _initialStopRisk));
            return moveProfitPercent;
        }

        #endregion
    }
}