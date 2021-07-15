using System;
using System.Collections.Generic;
using System.Linq;

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
        public decimal TradesSuccessesPercent => (TradesMade ==0 ?0: Math.Round((decimal) TradesSuccesses / TradesMade * 100m, 2));
        public double AvgDuration => TradesActive >0?0: Trades.Where(x => !x.IsActive).Average(x => (x.EndDate - x.StartDate).Hours);
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

        public Trade AddTrade(in DateTime date, in decimal price, decimal quantity)
        {
            var addTrade = new Trade(date, price, quantity);
            Trades.Add(addTrade);
            return addTrade;
        }

        #region Nested type: Trade

        public class Trade
        {
            public Trade(DateTime startDate, decimal buyPrice, decimal quantity)
            {
                StartDate = startDate;
                BuyPrice = buyPrice;
                Quantity = quantity;
                IsActive = true;
            }

            public decimal Value { get; set; }
            public DateTime StartDate { get; }
            public decimal BuyPrice { get; }
            public decimal Quantity { get; }
            public bool IsActive { get; private set; }
            public decimal SellPrice { get; private set; }
            public DateTime EndDate { get; private set; }
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
                return Math.Round( (currentValue - fromValue) / fromValue * 100 , decimals);
            }
        }

        #endregion
    }
}