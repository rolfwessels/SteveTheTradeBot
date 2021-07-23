using System;
using System.Collections.Generic;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class StrategyTrade : BaseDalModelWithGuid
    {
        public StrategyTrade()
        {
        }

        public StrategyTrade(DateTime startDate, decimal buyPrice, decimal quantity, decimal buyValue)
        {
            StartDate = startDate;
            BuyPrice = buyPrice;
            Quantity = quantity;
            BuyValue = buyValue;
            IsActive = true;
            Orders = new List<TradeOrder>();
        }

        public string StrategyInstanceId { get; set; }
        public List<TradeOrder> Orders { get; set; }
        public decimal Value { get; set; }
        public DateTime StartDate { get; }
        public decimal BuyPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal BuyValue { get; }
        public bool IsActive { get; set; }
        public decimal SellPrice { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Profit { get; set; }
        public string FeeCurrency { get; set; }
        public decimal FeeAmount { get; set; }

        public TradeOrder AddOrderRequest(Side side, decimal outQuantity, decimal estimatedPrice, decimal estimatedQuantity,
            string currencyPair, in DateTime requestDate)
        {
            var tradeOrder = new TradeOrder()
            {
                RequestDate = requestDate,
                OrderStatusType = OrderStatusTypes.Placed,
                CurrencyPair = currencyPair,
                PriceAtRequest = estimatedPrice,
                OrderPrice = estimatedPrice,
                OrderSide = side,
                RemainingQuantity = 0,
                OriginalQuantity = estimatedQuantity,
                OutQuantity = outQuantity,
                OutCurrency = currencyPair.SideOut(side),
            };
            Orders.Add(tradeOrder);
            return tradeOrder;
        }

        public void ApplyBuyInfo(TradeOrder tradeOrder)
        {
            BuyPrice = tradeOrder.OrderPrice;
            Quantity = tradeOrder.OriginalQuantity;
            FeeCurrency = tradeOrder.OutCurrency;
            FeeAmount = tradeOrder.SwapFeeAmount(FeeCurrency);
        }
    }
}