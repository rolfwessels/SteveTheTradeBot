using System;
using System.Collections.Generic;
using System.Linq;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class StrategyInstance : BaseDalModelWithGuid
    {
        public string Reference { get; set; }
        public string Feed { get; set; }
        public string Pair { get; set; }
        public PeriodSize PeriodSize { get; set; }
        public string StrategyName { get; set; }

        public List<StrategyTrade> Trades { get; set; } = new List<StrategyTrade>();

        public bool IsActive { get; set; }
        public bool IsBackTest { get; set; }
        public decimal InvestmentAmount { get; set; }
        public decimal BaseAmount { get; set; }
        public string BaseAmountCurrency { get; set; }
        public decimal QuoteAmount { get; set; }
        public string QuoteAmountCurrency { get; set; }
        

        public decimal TotalActiveTrades { get; set; }
        public decimal TotalNumberOfTrades { get; set; }
        public double AverageTradesPerMonth { get; set; }
        public decimal PercentOfProfitableTrades { get; set; }
        public decimal NumberOfProfitableTrades { get; set; }
        public decimal NumberOfLosingTrades { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalLoss { get; set; }
        public decimal TotalFee { get; set; }
        public decimal PercentProfit { get; set; }
        public decimal LargestProfit { get; set; }
        public decimal LargestLoss { get; set; }
        public decimal FirstClose { get; set; }
        public decimal LastClose { get; set; }
        public decimal PercentMarketProfit { get; set; }
        public TimeSpan AverageTimeInMarket { get; set; }
        public DateTime FirstStart { get; set; }
        public DateTime LastDate { get; set; }
        
        // public decimal MaximumDrawDown { get; set; }
        // public decimal MaximumDrawDownMonteCarlo { get; set; }
        // public decimal StandardDeviation { get; set; }
        // public decimal StandardDeviationAnnualized { get; set; }
        // public decimal DownsideDeviationMar10 { get; set; }
        // public decimal ValueAddedMonthlyIndex { get; set; }
        // public decimal SharpeRatio { get; set; }
        // public decimal SortinoRatioMAR5 { get; set; }
        // public decimal AnnualizedSortinoRatioMAR5 { get; set; }
        // public decimal SterlingRatioMAR5 { get; set; }
        // public decimal CalmarRatio { get; set; }
        // public decimal RiskRewardRatio { get; set; }

        public static StrategyInstance ForBackTest(string strategy, string pair)
        {
            var forBackTest = From( strategy,  pair, 1000, PeriodSize.FiveMinutes);
            forBackTest.IsBackTest = true;
            return forBackTest;
        }


        public StrategyTrade AddTrade( DateTime date, in decimal price, decimal quantity, decimal randValue)
        {
            var addTrade = new StrategyTrade(date, price, quantity, randValue);
            Trades.Add(addTrade);
            return addTrade;
        }


        public static StrategyInstance From(string strategy, string pair, decimal amount, PeriodSize periodSize)
        {
            return new StrategyInstance()
            {
                StrategyName = strategy,
                IsActive = true,
                IsBackTest = false,
                Pair = pair,
                PeriodSize = periodSize,
                InvestmentAmount = amount,
                BaseAmount = amount,
                BaseAmountCurrency = pair.SideIn(Side.Sell),
                QuoteAmount = 0,
                QuoteAmountCurrency = pair.SideIn(Side.Buy),
                Feed = "valr",
                Reference = $"{strategy}_{pair}_{periodSize}_{DateTime.Now:yyyyMMdd}".ToLower()
            };
        }

        public (StrategyTrade addTrade, TradeOrder tradeOrder) AddBuyTradeOrder(decimal randValue,
            decimal estimatedPrice, DateTime currentTradeDate)
        {
            var estimatedQuantity = randValue / estimatedPrice;
            var addTrade = AddTrade(currentTradeDate, estimatedPrice, estimatedQuantity, randValue);
            var tradeOrder = addTrade.AddOrderRequest(Side.Buy, randValue, estimatedPrice, estimatedQuantity,
                Pair, currentTradeDate);
            return (addTrade, tradeOrder);
        }
    }
}