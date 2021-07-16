using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public class BackTestResult
    {
        private decimal _startingAmount;
        public List<Trade> Trades { get; } = new List<Trade>();
        public string CurrencyPair { get; set; }
        public int TradesActive => Trades.Count(x => x.IsActive);
        public int TradesMade => Trades.Count(x => !x.IsActive);
        public int TradesSuccesses => Trades.Where(x => !x.IsActive).Count(x => x.Profit > 0);

        public decimal TradesSuccessesPercent =>
            (TradesMade == 0 ? 0 : Math.Round((decimal) TradesSuccesses / TradesMade * 100m, 2));

        public double AvgDuration => TradesActive > 0
            ? 0
            : Trades.Where(x => !x.IsActive).Average(x => (x.EndDate - x.StartDate)?.Hours ?? 0);

        public int DatePoints { get; set; }
        public int TotalTransactionCost { get; set; }


        public decimal StartingAmount
        {
            get => _startingAmount;
            set => _startingAmount = ClosingBalance = value;
        }

        public decimal ClosingBalance { get; set; }
        public decimal BalanceMoved => Trade.MovementPercent(ClosingBalance, StartingAmount);
        public decimal MarketOpenAt { get; set; }
        public decimal MarketClosedAt { get; set; }
        public decimal MarketMoved => Trade.MovementPercent(MarketClosedAt, MarketOpenAt);

        public Trade AddTrade(in DateTime date, in decimal price, decimal quantity, decimal randValue)
        {
            var addTrade = new Trade(date, price, quantity, randValue);
            Trades.Add(addTrade);
            return addTrade;
        }
    }


    public class Trade : BaseDalModelWithGuid
    {
        public Trade(DateTime startDate, decimal buyPrice, decimal quantity, decimal buyValue)
        {
            Id = Guid.NewGuid().ToString("n");
            StartDate = startDate;
            BuyPrice = buyPrice;
            Quantity = quantity;
            BuyValue = buyValue;
            IsActive = true;
            Orders = new List<TradeOrder>();
        }

        public List<TradeOrder> Orders { get; set; }
        public decimal Value { get; set; }
        public DateTime StartDate { get; }
        public decimal BuyPrice { get; }
        public decimal Quantity { get; }
        public decimal BuyValue { get; }
        public bool IsActive { get; private set; }
        public decimal SellPrice { get; private set; }
        public DateTime? EndDate { get; private set; }
        public decimal Profit { get; set; }


        public Trade Close(in DateTime endDate, in decimal sellPrice)
        {
            EndDate = endDate;
            Value = Math.Round(Quantity * sellPrice, 2);
            SellPrice = sellPrice;
            Profit = MovementPercent(sellPrice, BuyPrice);
            IsActive = false;
            return this;
        }

        public static decimal MovementPercent(decimal currentValue, decimal fromValue, int decimals = 3)
        {
            if (fromValue == 0) fromValue = 0.00001m;
            return Math.Round((currentValue - fromValue) / fromValue * 100, decimals);
        }

        public TradeOrder AddOrderRequest(Side side, decimal outQuantity, decimal priceAtRequest, decimal quantity,
            string currencyPair, in DateTime requestDate)
        {
            var tradeOrder = new TradeOrder()
            {
                RequestDate = requestDate,
                OrderStatusType = OrderStatusTypes.Placed,
                CurrencyPair = currencyPair,
                PriceAtRequest = priceAtRequest,
                OriginalPrice = priceAtRequest,
                OrderSide = side,
                RemainingQuantity = 0,
                OriginalQuantity = quantity,
                OutQuantity = outQuantity,
                OutCurrency = currencyPair.SideOut(side),
            };
            Orders.Add(tradeOrder);
            return tradeOrder;
        }
    }

    public class TradeOrder : BaseDalModelWithGuid
    {
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public OrderStatusTypes OrderStatusType { get; set; } = OrderStatusTypes.Placed;
        public string CurrencyPair { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal OriginalQuantity { get; set; }
        public Side OrderSide { get; set; }
        public string OrderType { get; set; }
        public string BrokerOrderId { get; set; }
        public string FailedReason { get; set; } = null;
        public decimal PriceAtRequest { get; set; }
        public decimal OutQuantity { get; set; }
        public string OutCurrency { get; set; }

        public MarketOrderRequest ToMarketOrderRequest()
        {
            return new MarketOrderRequest(OrderSide, OriginalQuantity, CurrencyPair, Id, RequestDate);
        }

        public void Apply(OrderStatusResponse response)
        {
            if (CurrencyPair != response.CurrencyPair) throw new Exception($"Currency pair does not match {Id} vs {response.CustomerOrderId}");
            if (Id != response.CustomerOrderId) throw new Exception($"Invalid status response given for this order {Id} vs {response.CustomerOrderId}");
            BrokerOrderId = response.OrderId;
            OrderStatusType = OrderStatusTypesHelper.ToOrderStatus(response.OrderStatusType);
            OriginalPrice = response.OriginalPrice;
            RemainingQuantity = response.RemainingQuantity;
            OriginalQuantity = response.OriginalQuantity;
            OrderType = response.OrderType;
            FailedReason = response.FailedReason;
        }
    }
}