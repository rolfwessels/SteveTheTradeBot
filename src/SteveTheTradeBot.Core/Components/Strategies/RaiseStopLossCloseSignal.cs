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
    public class RaiseStopLossCloseSignal : ICloseSignal
    {
        private readonly decimal _initialStopRisk;
        private readonly decimal _moveProfitPercent;
        private readonly decimal _secondStopRisk;

        public RaiseStopLossCloseSignal(decimal initialStopRisk = 0.96m, decimal moveProfitPercent = 1.05m)
        {
            _initialStopRisk = initialStopRisk;
            _secondStopRisk = initialStopRisk;
            _moveProfitPercent = moveProfitPercent;
        }

        
        private async Task<decimal> GetUpdateStopLossAt(StrategyTrade activeTrade, StrategyContext data)
        {
            var movePercent = await data.Get(StrategyProperty.UpdateStopLossAt,0);
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
            await SetStopLoss(data, strategy, lossAmount);
            return lossAmount;
        }

        private static async Task SetStopLoss(StrategyContext data, BaseStrategy strategy, decimal lossAmount)
        {
            await strategy.SetStopLoss(data, lossAmount);
            await data.Messenger.Send(PostSlackMessage.From($"{data.StrategyInstance.Name} set stop loss to {lossAmount}."));
        }

        public async Task DetectClose(StrategyContext data, TradeQuote currentTrade, StrategyTrade activeTrade,
            BaseStrategy strategy)
        {
            var updateStopLossAt = await GetUpdateStopLossAt(activeTrade, data);
            if (currentTrade.Close > updateStopLossAt)
            {
                var oldStopLoss = activeTrade.GetValidStopLoss().OrderPrice;
                var newStopLoss = currentTrade.Close * _secondStopRisk;
                await data.Set(StrategyProperty.UpdateStopLossAt, currentTrade.Close * _moveProfitPercent);
                await data.Set(StrategyProperty.StopLoss, newStopLoss);
                await strategy.SetStopLoss(data, newStopLoss);
                data.StrategyInstance.Status = $"Update stop loss to {newStopLoss} that means guaranteed profit of {TradeUtils.MovementPercent(newStopLoss, activeTrade.BuyValue)}%";
                await data.Messenger.Send(PostSlackMessage.From($"{data.StrategyInstance.Name} {data.StrategyInstance.Status} :chart_with_upwards_trend:"));
            }
            else
            {
                data.StrategyInstance.Status =
                    $"Waiting for price above {updateStopLossAt} or stop loss {activeTrade.GetValidStopLoss()?.OrderPrice}]";
            }
        }

        #endregion
    }
}