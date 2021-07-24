﻿using System;
using System.Collections.Generic;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class StrategyTrade : BaseDalModelWithGuid
    {
        public StrategyTrade()
        {
        }

        public StrategyTrade(DateTime startDate, decimal buyPrice, decimal buyQuantity, decimal buyValue)
        {
            StartDate = startDate;
            BuyPrice = buyPrice;
            BuyQuantity = buyQuantity;
            BuyValue = buyValue;
            IsActive = true;
            Orders = new List<TradeOrder>();
        }

        public string StrategyInstanceId { get; set; }
        public List<TradeOrder> Orders { get; set; }
        
        public DateTime StartDate { get; }
        
        public decimal BuyQuantity { get; set; }
        public decimal BuyValue { get; }
        public decimal BuyPrice { get; set; }

        public decimal SellValue { get; set; }
        public decimal SellPrice { get; set; }

        public bool IsActive { get; set; }
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
            BuyQuantity = tradeOrder.OriginalQuantity;
            FeeCurrency = tradeOrder.OutCurrency;
            FeeAmount = tradeOrder.SwapFeeAmount(FeeCurrency);
        }
    }
}