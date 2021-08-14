using System;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.Slack;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class FollowStopLossOutCloseSignal : ICloseSignal
    {
        private readonly decimal _initialStopRisk;
        private readonly decimal _moveProfitPercent;
        private readonly decimal _secondStopRisk;

        public FollowStopLossOutCloseSignal(decimal initialStopRisk = 0.96m, decimal moveProfitPercent = 1.05m)
        {
            _initialStopRisk = initialStopRisk;
            _secondStopRisk = initialStopRisk;
            _moveProfitPercent = moveProfitPercent;
        }

        
        private async Task<decimal> GetMoveProfit(StrategyTrade activeTrade, StrategyContext data)
        {
            var movePercent = await data.Get(StrategyProperty.UpdateStopLossAt, await data.Get("movePercent", 0));
            if (movePercent != 0) return movePercent;
            var validStopLoss = activeTrade.GetValidStopLoss();
            if (validStopLoss == null) return activeTrade.BuyPrice;
            var moveProfitPercent = validStopLoss.OrderPrice * (_moveProfitPercent + (1 - _initialStopRisk));
            return moveProfitPercent;
        }


        #region Implementation of ICloseSignal

        public async Task<decimal> Initialize(StrategyContext data, decimal boughtAtPrice, BaseStrategy strategy)
        {
            var lossAmount = boughtAtPrice * _initialStopRisk;
            await data.Set(StrategyProperty.StopLoss, lossAmount);
            await data.Set(StrategyProperty.UpdateStopLossAt, boughtAtPrice * _moveProfitPercent);
            await strategy.SetStopLoss(data, lossAmount);
            await data.Messenger.Send(new PostSlackMessage() { Message = $"{data.StrategyInstance.Name} set stop loss to {lossAmount}." });
            return lossAmount;
        }

        public async Task DetectClose(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade,
            BaseStrategy strategy)
        {
            var moveProfit = await GetMoveProfit(activeTrade, data);
            if (currentTrade.Close > moveProfit)
            {
                var oldStopLoss = activeTrade.GetValidStopLoss().OrderPrice;
                var newLossAmount = currentTrade.Close * _secondStopRisk;
                await data.Set(StrategyProperty.UpdateStopLossAt, currentTrade.Close * _moveProfitPercent);
                await data.Set(StrategyProperty.StopLoss, newLossAmount);
                await strategy.SetStopLoss(data, newLossAmount);
                data.StrategyInstance.Status = $"Update stop loss to {newLossAmount} by {TradeUtils.MovementPercent(newLossAmount, oldStopLoss)}%";
                await data.Messenger.Send(
                    $"{data.StrategyInstance.Name} {data.StrategyInstance.Status} :chart_with_upwards_trend:");
            }
            else
            {
                data.StrategyInstance.Status =
                    $"Waiting for price above {moveProfit} or stop loss {activeTrade.GetValidStopLoss()?.OrderPrice}]";
            }
        }

        #endregion
    }
}