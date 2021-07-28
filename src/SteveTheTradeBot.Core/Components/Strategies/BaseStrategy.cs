using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public abstract class BaseStrategy : IStrategy
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        #region Implementation of IBot

        public abstract Task DataReceived(StrategyContext data);
        public abstract string Name { get; }
        public Task SellAll(StrategyContext strategyContext)
        {
            var enumerable = strategyContext.StrategyInstance.Trades.Where(x => x.IsActive).Select(x => Sell(strategyContext, x));
            return Task.WhenAll(enumerable);
        }

        #endregion

        public async Task<StrategyTrade> Buy(StrategyContext data, decimal randValue)
        {
            var currentTrade = data.LatestQuote();
            var estimatedPrice = currentTrade.Close;
            var currentTradeDate = currentTrade.Date;
            var (addTrade, tradeOrder) = data.StrategyInstance.AddBuyTradeOrder(randValue, estimatedPrice, currentTradeDate);
            try
            {
                var response = await data.Broker.Order(BrokerUtils.ToOrderRequest(tradeOrder));
                BrokerUtils.ApplyValue(tradeOrder, response, Side.Sell);
                addTrade.ApplyBuyInfo(tradeOrder);
                BrokerUtils.ApplyBuyToStrategy(data, tradeOrder);
                await data.PlotRunData(currentTradeDate, "activeTrades", 1);
                await data.PlotRunData(currentTradeDate, "sellPrice", randValue);
                await data.Messenger.Send(new TradeOrderMadeMessage(data.StrategyInstance, addTrade, tradeOrder));
            }
            catch (Exception e)
            {
                tradeOrder.FailedReason = e.Message;
                tradeOrder.OrderStatusType = OrderStatusTypes.Failed;
                BrokerUtils.ApplyCloseToActiveTrade(addTrade, currentTradeDate, tradeOrder);
                _log.Error(e, $"Failed to add new trade order:{e.Message}");
            }
            return addTrade;
        }


        public async Task SetStopLoss(StrategyContext data, decimal stopLossAmount)
        {
            
            var activeTrade = data.ActiveTrade();
            if (activeTrade.GetValidStopLoss() != null) await CancelStopLoss(data, activeTrade.GetValidStopLoss());
            var currentTrade = data.LatestQuote();
            var estimatedQuantity = 0;
            var tradeOrder = activeTrade.AddOrderRequest(Side.Sell, activeTrade.BuyQuantity, stopLossAmount, estimatedQuantity, data.StrategyInstance.Pair, currentTrade.Date, currentTrade.Close);
            tradeOrder.OrderType = StrategyTrade.OrderTypeStopLoss;
            var lossAmount = stopLossAmount * 0.99m;
            tradeOrder.StopPrice = lossAmount;
            try
            {
                var response = await data.Broker.StopLimitOrder(new StopLimitOrderRequest(tradeOrder.OrderSide, activeTrade.BuyQuantity, stopLossAmount, data.StrategyInstance.Pair, tradeOrder.Id, TimeEnforce.FillOrKill, lossAmount, StopLimitOrderRequest.Types.StopLossLimit));
                tradeOrder.BrokerOrderId = response.Id;
            }
            catch (Exception e)
            {
                tradeOrder.FailedReason = e.Message;
                tradeOrder.OrderStatusType = OrderStatusTypes.Failed;
                _log.Error(e, $"Failed to SetStopLoss order:{e.Message}");
            }
            
        }

        private async Task CancelStopLoss(StrategyContext data, TradeOrder tradeOrder)
        {
            try
            {
                await data.Broker.CancelOrder(tradeOrder.BrokerOrderId);
                tradeOrder.OrderStatusType = OrderStatusTypes.Cancelled;
            }
            catch (Exception e)
            {
                tradeOrder.FailedReason = e.Message;
                tradeOrder.OrderStatusType = OrderStatusTypes.Failed;
                _log.Error(e, $"Failed to CancelStopLoss order:{e.Message}");
            }
        }


        public async Task Sell(StrategyContext data, StrategyTrade activeTrade)
        {
            var currentTrade = data.LatestQuote();
            var estimatedPrice = currentTrade.Close;
            var estimatedQuantity = activeTrade.BuyQuantity * estimatedPrice;
            var tradeOrder = activeTrade.AddOrderRequest(Side.Sell, activeTrade.BuyQuantity, estimatedPrice, estimatedQuantity, data.StrategyInstance.Pair, currentTrade.Date, estimatedPrice);
            try
            {
                var response = await data.Broker.Order(BrokerUtils.ToOrderRequest(tradeOrder));
                BrokerUtils.ApplyValue(tradeOrder, response, Side.Buy);
                var close = BrokerUtils.ApplyCloseToActiveTrade(activeTrade, currentTrade.Date, tradeOrder);
                BrokerUtils.ApplyCloseToStrategy(data, close);
                await data.PlotRunData(currentTrade.Date, "activeTrades", 0);
                await data.PlotRunData(currentTrade.Date, "sellPrice", close.SellValue);
                await data.Messenger.Send(new TradeOrderMadeMessage(data.StrategyInstance, activeTrade, tradeOrder));
            }
            catch (Exception e)
            {
                tradeOrder.FailedReason = e.Message;
                tradeOrder.OrderStatusType = OrderStatusTypes.Failed;
                _log.Error(e, $"Failed to add close trade order:{e.Message}");
            }
            
        }
    }
}
