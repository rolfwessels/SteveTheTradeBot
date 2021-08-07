using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public class BrokerUtils
    {
        public static StrategyTrade ApplyCloseToActiveTrade(StrategyTrade trade, DateTime endDate, TradeOrder tradeOrder)
        {
            trade.EndDate = tradeOrder.UpdateDate;
            trade.SellValue = tradeOrder.OriginalQuantity;
            trade.SellPrice = tradeOrder.OrderPrice;
            trade.Profit = TradeUtils.MovementPercent(tradeOrder.OriginalQuantity, trade.BuyValue);
            trade.IsActive = false;
            trade.FeeAmount += tradeOrder.SwapFeeAmount(tradeOrder.FeeCurrency);
            trade.FeeCurrency = tradeOrder.FeeCurrency;
            return trade;
        }

        public static MarketOrderRequest ToMarketOrderRequest(TradeOrder tradeOrder)
        {
            return new MarketOrderRequest(tradeOrder.OrderSide,null, tradeOrder.OriginalQuantity, tradeOrder.CurrencyPair, tradeOrder.Id, tradeOrder.RequestDate);
        }

        public static void Apply(TradeOrder tradeOrder, OrderStatusResponse response)
        {
            if (tradeOrder.CurrencyPair != response.CurrencyPair) throw new Exception($"Currency pair does not match {tradeOrder.Id} vs {response.CustomerOrderId}");
            if (tradeOrder.Id != response.CustomerOrderId) throw new Exception($"Invalid status response given for this order {tradeOrder.Id} vs {response.CustomerOrderId}");
            tradeOrder.BrokerOrderId = response.OrderId;
            tradeOrder.OrderStatusType = OrderStatusTypesHelper.ToOrderStatus(response.OrderStatusType);
            tradeOrder.OrderPrice = response.OriginalPrice;
            tradeOrder.RemainingQuantity = response.RemainingQuantity;
            tradeOrder.OriginalQuantity = response.OriginalQuantity;
            tradeOrder.OrderType = response.OrderType;
            tradeOrder.FailedReason = response.FailedReason;
        }

        public static SimpleOrderRequest ToOrderRequest(TradeOrder tradeOrder)
        {
            return SimpleOrderRequest.From(tradeOrder.OrderSide, tradeOrder.Total, tradeOrder.CurrencyPair.SideOut(tradeOrder.OrderSide), tradeOrder.RequestDate, tradeOrder.Id, tradeOrder.CurrencyPair);
        }

        public static void ActivateStopLoss(StrategyTrade trade, DateTime endDate, TradeOrder tradeOrder)
        {
            tradeOrder.OrderStatusType = OrderStatusTypes.Filled;
            ApplyCloseToActiveTrade(trade, endDate, tradeOrder);
        }


        public static async Task ActivateStopLoss(StrategyContext strategyContext, StrategyTrade trade, TradeOrder validStopLoss, OrderHistorySummaryResponse orderStatusById)
        {
            var orderStatusTypes = OrderStatusTypesHelper.ToOrderStatus(orderStatusById.OrderStatusType);
            switch (orderStatusTypes)
            {
                case OrderStatusTypes.Filled:
                    var currentTrade = strategyContext.LatestQuote();
                    ApplyValue(validStopLoss, orderStatusById);
                    DateTime endDate = strategyContext.LatestQuote().Date;
                    ApplyCloseToActiveTrade(trade, endDate, validStopLoss);
                    ApplyCloseToStrategy(strategyContext, trade);
                    await strategyContext.PlotRunData(currentTrade.Date, "activeTrades", 0);
                    await strategyContext.PlotRunData(currentTrade.Date, "sellPrice", trade.SellValue);
                    await strategyContext.Messenger.Send(new TradeOrderMadeMessage(strategyContext.StrategyInstance, trade, validStopLoss));
                    break;
                case OrderStatusTypes.PartiallyFilled:
                case OrderStatusTypes.Placed:
                    break;
                case OrderStatusTypes.Failed:
                    validStopLoss.OrderStatusType = OrderStatusTypes.Failed;
                    validStopLoss.FailedReason = orderStatusById.FailedReason;
                    break;
                case OrderStatusTypes.Cancelled:
                    validStopLoss.OrderStatusType = OrderStatusTypes.Cancelled;
                    validStopLoss.FailedReason = orderStatusById.FailedReason;
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

        public static async Task ActivateStopLoss(StrategyContext strategyContext, StrategyTrade trade,
            TradeOrder validStopLoss, decimal buyFeePercent)
        {
            var totalAmount = validStopLoss.Total * validStopLoss.OrderPrice;
            var feeAmount = Math.Round(totalAmount * buyFeePercent, 2);
            var receivedAmount = Math.Round(totalAmount - feeAmount, 2);
            
            var orderStatusById = new OrderHistorySummaryResponse() {
                OrderId = Gu.Id(),
                OrderStatusType = "Filled",
                CurrencyPair = strategyContext.StrategyInstance.Pair,
                OriginalPrice = validStopLoss.OrderPrice,
                AveragePrice = validStopLoss.OrderPrice,
                RemainingQuantity = 0,
                OriginalQuantity = receivedAmount,
                OrderSide = Side.Sell,
                OrderType = StrategyTrade.OrderTypeStopLoss,
                CustomerOrderId = validStopLoss.Id,
                TotalFee = feeAmount,
                Total = validStopLoss.Total,
                FeeCurrency = validStopLoss.FeeCurrency,
                FailedReason = "",
            };
            await ActivateStopLoss(strategyContext, trade, validStopLoss, orderStatusById);
        }

        public static void ApplyCloseToStrategy(StrategyContext data, StrategyTrade close)
        {
            data.StrategyInstance.QuoteAmount += close.SellValue;
            data.StrategyInstance.BaseAmount -= close.BuyQuantity;
        }

        public static void ApplyBuyToStrategy(StrategyContext data, TradeOrder tradeOrder)
        {
            data.StrategyInstance.QuoteAmount -= tradeOrder.Total;
            data.StrategyInstance.BaseAmount += tradeOrder.OriginalQuantity;
        }


        
    }
}