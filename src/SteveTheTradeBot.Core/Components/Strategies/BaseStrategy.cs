using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
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
            var (trade, order) = data.StrategyInstance.AddBuyTradeOrder(randValue, estimatedPrice, currentTradeDate);
            try
            {
              
                var response = await data.Broker.MarketOrder(BrokerUtils.ToOrderRequest(order));
                BrokerUtils.ApplyValue(order, response);
                trade.ApplyBuyInfo(order);
                BrokerUtils.ApplyBuyToStrategy(data, order);
                await data.PlotRunData(currentTradeDate, "activeTrades", 1);
                await data.PlotRunData(currentTradeDate, "sellPrice", randValue);
                await data.Messenger.Send(new TradeOrderMadeMessage(data.StrategyInstance, trade, order));
            }
            catch (Exception e)
            {
                order.FailedReason = e.Message;
                order.OrderStatusType = OrderStatusTypes.Failed;
                BrokerUtils.ApplyCloseToActiveTrade(trade, currentTradeDate, order);
                _log.Error(e, $"Failed to add new trade order:{e.Message}");
            }
            return trade;
        }


        public async Task SetStopLoss(StrategyContext data, decimal stopLossAmount)
        {
            
            var activeTrade = data.ActiveTrade();
            if (activeTrade.GetValidStopLoss() != null) await CancelStopLoss(data, activeTrade.GetValidStopLoss());
            var currentTrade = data.LatestQuote();
            var estimatedQuantity = 0;
            var tradeOrder = activeTrade.AddOrderRequest(Side.Sell, activeTrade.BuyQuantity, stopLossAmount, estimatedQuantity, data.StrategyInstance.Pair, currentTrade.Date, currentTrade.Close);
            tradeOrder.OrderType = StrategyTrade.OrderTypeStopLoss;
            var lossAmount = stopLossAmount * 1.01m;
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
                await data.Broker.CancelOrder(tradeOrder.BrokerOrderId, tradeOrder.CurrencyPair);
                tradeOrder.OrderStatusType = OrderStatusTypes.Cancelled;
            }
            catch (Exception e)
            {
                tradeOrder.FailedReason = e.Message;
                tradeOrder.OrderStatusType = OrderStatusTypes.Failed;
                _log.Error(e, $"Failed to CancelStopLoss order:{e.Message}");
            }
        }


        public async Task Sell(StrategyContext data, StrategyTrade trade)
        {
            var currentTrade = data.LatestQuote();
            var estimatedPrice = currentTrade.Close;
            var estimatedQuantity = trade.BuyQuantity * estimatedPrice;
            var order = trade.AddOrderRequest(Side.Sell, trade.BuyQuantity, estimatedPrice, estimatedQuantity, data.StrategyInstance.Pair, currentTrade.Date, estimatedPrice);
            try
            {
                var simpleOrderRequest = BrokerUtils.ToOrderRequest(order);
                var response = await data.Broker.MarketOrder(simpleOrderRequest);
                BrokerUtils.ApplyValue(order, response);
                var close = BrokerUtils.ApplyCloseToActiveTrade(trade, currentTrade.Date, order);
                BrokerUtils.ApplyCloseToStrategy(data, close);
                await data.PlotRunData(currentTrade.Date, "activeTrades", 0);
                await data.PlotRunData(currentTrade.Date, "sellPrice", close.SellValue);
                await data.Messenger.Send(new TradeOrderMadeMessage(data.StrategyInstance, trade, order));
            }
            catch (Exception e)
            {
                order.FailedReason = e.Message;
                order.OrderStatusType = OrderStatusTypes.Failed;
                _log.Error(e, $"Failed to add close trade order:{e.Message}");
            }
            
        }
    }
}
