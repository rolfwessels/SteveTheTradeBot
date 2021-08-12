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
    public abstract class FollowStopLossOutStrategyBase : BaseStrategy
    {
        private readonly decimal _initialStopRisk;
        private readonly decimal _moveProfitPercent;
        private readonly decimal _secondStopRisk;

        protected FollowStopLossOutStrategyBase(decimal initialStopRisk, decimal moveProfitPercent)
        {
            _initialStopRisk = initialStopRisk;
            _secondStopRisk = initialStopRisk;
            _moveProfitPercent = moveProfitPercent;
        }

        public async Task FollowClosingStrategy(StrategyContext data,  TradeQuote currentTrade, StrategyTrade activeTrade)
        {
            var moveProfit = await GetMoveProfit(activeTrade, data);
            if (currentTrade.Close > moveProfit)
            {
                var oldStopLoss = activeTrade.GetValidStopLoss().OrderPrice;
                var newLossAmount = currentTrade.Close * _secondStopRisk;
                await data.Set("movePercent", currentTrade.Close * _moveProfitPercent);
                await data.Set("currentStopLoss", newLossAmount);
                await SetStopLoss(data, newLossAmount);
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

        private async Task<decimal> GetMoveProfit(StrategyTrade activeTrade, StrategyContext data)
        {
            var movePercent = await data.Get("movePercent", 0);
            if (movePercent.GetValueOrDefault() != 0) return movePercent.Value;
            var validStopLoss = activeTrade.GetValidStopLoss();
            if (validStopLoss != null)
            {
                var moveProfitPercent = validStopLoss.OrderPrice * (_moveProfitPercent + (1 - _initialStopRisk));
                return moveProfitPercent;
            }
            return activeTrade.BuyPrice;
        }

        protected async Task<decimal> ResetStops(StrategyContext data, decimal currentBuyPrice)
        {
            var lossAmount = currentBuyPrice * _initialStopRisk;
            await data.Set("currentStopLoss", lossAmount);
            await data.Set("movePercent", currentBuyPrice * _moveProfitPercent);

            await SetStopLoss(data, lossAmount);
            await data.Messenger.Send(new PostSlackMessage() { Message = $"{data.StrategyInstance.Name} set stop loss to {lossAmount}." });
            return lossAmount;
        }


        protected Task<decimal> SetFirstStopLossFromPrice(StrategyContext data, decimal currentBuyPrice)
        {
            return ResetStops(data, currentBuyPrice);
        }
    }
}