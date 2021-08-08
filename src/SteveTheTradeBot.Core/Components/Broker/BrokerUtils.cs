using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SlackConnector.Models;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.Slack;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public class BrokerUtils
    {
        public static StrategyTrade ApplyCloseToActiveTrade(StrategyTrade trade, DateTime endDate, TradeOrder tradeOrder)
        {
            trade.EndDate = tradeOrder.UpdateDate;
            trade.SellValue = tradeOrder.TotalMinusFee;
            trade.SellPrice = tradeOrder.OrderPrice;
            trade.Profit = TradeUtils.MovementPercent(trade.SellValue, trade.BuyValue);
            trade.IsActive = false;
            trade.FeeAmount += tradeOrder.SwapFeeAmount(tradeOrder.FeeCurrency);
            trade.FeeCurrency = tradeOrder.FeeCurrency;
            return trade;
        }

        public static MarketOrderRequest ToMarketOrderRequest(TradeOrder tradeOrder)
        {
            return new MarketOrderRequest(tradeOrder.OrderSide,null, tradeOrder.OriginalQuantity, tradeOrder.CurrencyPair, tradeOrder.Id, tradeOrder.RequestDate);
        }



        // public static void Apply(TradeOrder tradeOrder, OrderStatusResponse response)
        // {
        //     if (tradeOrder.CurrencyPair != response.CurrencyPair) throw new Exception($"Currency pair does not match {tradeOrder.Id} vs {response.CustomerOrderId}");
        //     if (tradeOrder.Id != response.CustomerOrderId) throw new Exception($"Invalid status response given for this order {tradeOrder.Id} vs {response.CustomerOrderId}");
        //     tradeOrder.BrokerOrderId = response.OrderId;
        //     tradeOrder.OrderStatusType = OrderStatusTypesHelper.ToOrderStatus(response.OrderStatusType);
        //     tradeOrder.OrderPrice = response.OriginalPrice;
        //     tradeOrder.RemainingQuantity = response.RemainingQuantity;
        //     tradeOrder.OriginalQuantity = response.OriginalQuantity;
        //     tradeOrder.OrderType = response.OrderType;
        //     tradeOrder.FailedReason = response.FailedReason;
        // }

        public static SimpleOrderRequest ToOrderRequest(TradeOrder tradeOrder)
        {
            return SimpleOrderRequest.From(tradeOrder.OrderSide, tradeOrder.Total, tradeOrder.CurrencyPair.SideOut(tradeOrder.OrderSide), tradeOrder.RequestDate, tradeOrder.Id, tradeOrder.CurrencyPair);
        }

        // public static void ApplyOrderResultToStrategy(StrategyTrade trade, DateTime endDate, TradeOrder tradeOrder)
        // {
        //     tradeOrder.OrderStatusType = OrderStatusTypes.Filled;
        //     ApplyCloseToActiveTrade(trade, endDate, tradeOrder);
        // }

        public static async Task ApplyOrderResultToStrategy(StrategyContext strategyContext, StrategyTrade trade, TradeOrder order, OrderHistorySummaryResponse response)
        {
            var orderStatusTypes = OrderStatusTypesHelper.ToOrderStatus(response.OrderStatusType);
            switch (orderStatusTypes)
            {
                case OrderStatusTypes.Filled:
                    var currentTrade = strategyContext.LatestQuote();
                    ApplyValue(order, response);
                    ApplyCloseToActiveTrade(trade, currentTrade.Date, order);
                    ApplyCloseToStrategy(strategyContext, trade);
                    await strategyContext.PlotRunData(currentTrade.Date, "activeTrades", 0);
                    await strategyContext.PlotRunData(currentTrade.Date, "sellPrice", trade.SellValue);
                    await strategyContext.Messenger.Send(new TradeOrderMadeMessage(strategyContext.StrategyInstance, trade, order));
                    break;
                case OrderStatusTypes.PartiallyFilled:
                case OrderStatusTypes.Placed:
                    break;
                case OrderStatusTypes.Failed:
                case OrderStatusTypes.Cancelled:
                    order.OrderStatusType = orderStatusTypes;
                    order.FailedReason = response.FailedReason;
                    order.BrokerOrderId = response.OrderId;
                    await strategyContext.Messenger.Send(PostSlackMessage.From($"{strategyContext.StrategyInstance.Name} tried to place {order.OrderSide} for {order.Total} but has {orderStatusTypes}: {response.FailedReason}  "));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void ApplyValue(TradeOrder tradeOrder, OrderHistorySummaryResponse response)
        {   
            tradeOrder.BrokerOrderId = response.OrderId;
            tradeOrder.OrderStatusType = OrderStatusTypesHelper.ToOrderStatus(response.OrderStatusType);
            tradeOrder.OrderPrice = response.AveragePrice;
            tradeOrder.RemainingQuantity = response.RemainingQuantity;
            tradeOrder.OriginalQuantity = response.OriginalQuantity;
            tradeOrder.OrderType = response.OrderType;
            tradeOrder.Total = response.Total;
            tradeOrder.TotalFee = response.TotalFee;
            tradeOrder.FeeCurrency = response.FeeCurrency;
            tradeOrder.UpdateDate = response.OrderUpdatedAt;
        }

        public static async Task ApplyOrderResultToStrategy(StrategyContext strategyContext, StrategyTrade trade,
            TradeOrder order, decimal buyFeePercent)
        {
            var totalAmount = order.Total * order.OrderPrice;
            var feeAmount = Math.Round(totalAmount * buyFeePercent, 2);
            var receivedAmount = Math.Round(totalAmount - feeAmount, 2);
            
            var orderStatusById = new OrderHistorySummaryResponse() {
                OrderId = Gu.Id(),
                OrderStatusType = "Filled",
                CurrencyPair = strategyContext.StrategyInstance.Pair,
                OriginalPrice = order.OrderPrice,
                AveragePrice = order.OrderPrice,
                RemainingQuantity = 0,
                OriginalQuantity = order.Total,
                OrderSide = Side.Sell,
                OrderType = StrategyTrade.OrderTypeStopLoss,
                CustomerOrderId = order.Id,
                TotalFee = feeAmount,
                Total = receivedAmount,
                FeeCurrency = order.FeeCurrency,
                FailedReason = "",
            }.Dump("");
            await ApplyOrderResultToStrategy(strategyContext, trade, order, orderStatusById);
        }

        public static void ApplyBuyInfo(StrategyTrade strategyTrade, TradeOrder tradeOrder)
        {
            strategyTrade.BuyPrice = tradeOrder.OrderPrice;
            strategyTrade.BuyQuantity = tradeOrder.QuantityMinusFee;
            strategyTrade.FeeCurrency = tradeOrder.CurrencyPair.QuoteCurrency();
            strategyTrade.FeeAmount = tradeOrder.SwapFeeAmount(tradeOrder.FeeCurrency);
        }

        public static void ApplyCloseToStrategy(StrategyContext data, StrategyTrade close)
        {
            data.StrategyInstance.QuoteAmount += close.SellValue;
            data.StrategyInstance.BaseAmount -= close.BuyQuantity;
        }

        public static void ApplyBuyToStrategy(StrategyContext data, TradeOrder tradeOrder)
        {
            data.StrategyInstance.QuoteAmount -= tradeOrder.Total;
            data.StrategyInstance.BaseAmount += tradeOrder.QuantityMinusFee;
        }


        
    }
}