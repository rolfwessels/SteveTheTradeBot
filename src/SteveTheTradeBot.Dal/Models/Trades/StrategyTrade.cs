using System;
using System.Collections.Generic;
using System.Linq;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class StrategyTrade : BaseDalModelWithGuid
    {
        public const string OrderTypeStopLoss = "stop-loss-limit";

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

        public DateTime StartDate { get; set; }

        public decimal BuyQuantity { get; set; }
        public decimal BuyValue { get; set; }
        public decimal BuyPrice { get; set; }

        public decimal SellValue { get; set; }
        public decimal SellPrice { get; set; }

        public bool IsActive { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Profit { get; set; }
        public string FeeCurrency { get; set; }
        public decimal FeeAmount { get; set; }

        public TradeOrder AddOrderRequest(Side side, decimal outQuantity, decimal estimatedPrice,
            decimal estimatedQuantity,
            string currencyPair, in DateTime requestDate, decimal priceAtRequest)
        {
            var tradeOrder = new TradeOrder()
            {
                RequestDate = requestDate,
                OrderStatusType = OrderStatusTypes.Placed,
                CurrencyPair = currencyPair,
                PriceAtRequest = priceAtRequest,
                OrderPrice = estimatedPrice,
                StopPrice = 0,
                OrderSide = side,
                RemainingQuantity = 0,
                OriginalQuantity = estimatedQuantity,
                Total = outQuantity,
                FeeCurrency = currencyPair.SideIn(side)
            };
            Orders.Add(tradeOrder);
            return tradeOrder;
        }

        public decimal PriceDifference()
        {
            return Math.Abs(BuyValue - SellValue);
        }

        public bool IsProfit()
        {
            return Profit > 0;
        }

        public TradeOrder GetValidStopLoss()
        {
            return Orders.FirstOrDefault(x =>
                x.OrderType == OrderTypeStopLoss && x.OrderStatusType == OrderStatusTypes.Placed);
        }

        public string ToString(StrategyInstance forBackTest)
        {
            if (IsActive) 
                return $"Bought {Amount.From(BuyQuantity, forBackTest.Pair.BaseCurrency())} at {Amount.From(BuyPrice, forBackTest.Pair.QuoteCurrency())} for {Amount.From(BuyValue, forBackTest.Pair.QuoteCurrency())} (Fee {Amount.From(FeeAmount,FeeCurrency)})";

            return $"Sold {Amount.From(BuyQuantity, forBackTest.Pair.BaseCurrency())} at {Amount.From(SellPrice, forBackTest.Pair.QuoteCurrency())} for {Amount.From(SellValue, forBackTest.Pair.QuoteCurrency())} (Fee {Amount.From(FeeAmount, FeeCurrency)})";
        }
    }
}