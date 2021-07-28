using System;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public class BrokerUtils
    {
        public static StrategyTrade ApplyCloseToActiveTrade(StrategyTrade trade, DateTime endDate, TradeOrder tradeOrder)
        {
            trade.EndDate = tradeOrder.RequestDate;
            trade.SellValue = tradeOrder.OriginalQuantity;
            trade.SellPrice = tradeOrder.OrderPrice;
            trade.Profit = TradeUtils.MovementPercent(tradeOrder.OriginalQuantity, trade.BuyValue);
            trade.IsActive = false;
            trade.FeeAmount += tradeOrder.SwapFeeAmount(trade.FeeCurrency);

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
            return SimpleOrderRequest.From(tradeOrder.OrderSide, tradeOrder.OutQuantity, tradeOrder.OutCurrency, tradeOrder.RequestDate, tradeOrder.Id, tradeOrder.CurrencyPair);
        }

        public static void ActivateStopLoss(StrategyTrade trade, DateTime endDate, TradeOrder tradeOrder)
        {
            tradeOrder.OrderStatusType = OrderStatusTypes.Filled;
            ApplyCloseToActiveTrade(trade, endDate, tradeOrder);
        }

        public static void ApplyValue(TradeOrder tradeOrder, SimpleOrderStatusResponse response, Side sell)
        {
            tradeOrder.BrokerOrderId = response.OrderId;
            tradeOrder.OrderStatusType = response.Success ? OrderStatusTypes.Filled : ((response.Processing) ? OrderStatusTypes.Placed : OrderStatusTypes.Failed);
            tradeOrder.OrderPrice = response.OriginalPrice(sell);
            tradeOrder.RemainingQuantity = 0;
            tradeOrder.OriginalQuantity = response.ReceivedAmount;
            tradeOrder.OrderType = "simple";
            tradeOrder.FeeAmount = response.FeeAmount;
            tradeOrder.FeeCurrency = response.FeeCurrency;
        }
        
        public static void ActivateStopLoss(StrategyContext strategyContext, StrategyTrade activeTrades,
            TradeOrder validStopLoss, decimal buyFeePercent)
        {
            var totalAmount = validStopLoss.OutQuantity * validStopLoss.StopPrice;
            var feeAmount = Math.Round(totalAmount * buyFeePercent, 2);
            var receivedAmount = Math.Round(totalAmount - feeAmount, 2);
            validStopLoss.OriginalQuantity = receivedAmount;
            validStopLoss.FeeAmount = feeAmount;
            activeTrades.FeeCurrency = validStopLoss.FeeCurrency;
            ActivateStopLoss(activeTrades, strategyContext.LatestQuote().Date, validStopLoss);
            ApplyCloseToStrategy(strategyContext, activeTrades);
        }

        public static void ApplyCloseToStrategy(StrategyContext data, StrategyTrade close)
        {
            data.StrategyInstance.BaseAmount += close.SellValue;
            data.StrategyInstance.QuoteAmount -= close.BuyQuantity;
        }

        public static void ApplyBuyToStrategy(StrategyContext data, TradeOrder tradeOrder)
        {
            data.StrategyInstance.BaseAmount -= tradeOrder.OutQuantity;
            data.StrategyInstance.QuoteAmount += tradeOrder.OriginalQuantity;
        }
    }
}