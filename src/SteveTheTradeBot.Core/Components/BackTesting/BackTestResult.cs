using System;
using System.Collections.Generic;
using System.Linq;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public class BackTestResult
    {
        private decimal _startingAmount;
        public List<StrategyTrade> Trades { get; } = new List<StrategyTrade>();
        public string CurrencyPair { get; set; }
        public int TradesActive => Trades.Count(x => x.IsActive);
        public int TradesMade => Trades.Count(x => !x.IsActive);
        public int TradesSuccesses => Trades.Where(x => !x.IsActive).Count(x => x.Profit > 0);

        public decimal TradesSuccessesPercent =>
            (TradesMade == 0 ? 0 : Math.Round((decimal) TradesSuccesses / TradesMade * 100m, 2));

        public TimeSpan AvgDuration => TradesMade > 0
            ? TimeSpan.FromSeconds(0)
            : TimeSpan.FromHours(Trades.Where(x => !x.IsActive).Average(x => (x.EndDate - x.StartDate)?.Hours ?? 0));

        public int DatePoints { get; set; }
        public decimal TotalTransactionCost => Trades.Sum(x => x.FeeAmount);
        public decimal ClosingBalance { get; set; }
        public decimal BalanceMoved => TradeUtils.MovementPercent(ClosingBalance, StartingAmount);
        public decimal MarketOpenAt { get; set; }
        public decimal MarketClosedAt { get; set; }
        public decimal MarketMoved => TradeUtils.MovementPercent(MarketClosedAt, MarketOpenAt);

        public decimal StartingAmount
        {
            get => _startingAmount;
            set => _startingAmount = ClosingBalance = value;
        }

        

        public StrategyTrade AddTrade(in DateTime date, in decimal price, decimal quantity, decimal randValue)
        {
            var addTrade = new StrategyTrade(date, price, quantity, randValue);
            Trades.Add(addTrade);
            return addTrade;
        }
    }


    

}