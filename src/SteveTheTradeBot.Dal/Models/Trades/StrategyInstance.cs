using System;
using System.Collections.Generic;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class StrategyInstance : BaseDalModelWithGuid
    {
        public string Reference { get; set; }
        public List<StrategyTrade> Trades { get; set; } = new List<StrategyTrade>();
        public string StrategyName { get; set; }
        public string Pair { get; set; }
        public PeriodSize PeriodSize { get; set; }
        public bool IsActive { get; set; }
        public bool IsBackTest { get; set; }
        public decimal StartingAmount { get; set; }
        public decimal BaseAmount { get; set; }
        public string BaseAmountCurrency { get; set; }
        public decimal QuoteAmount { get; set; }
        public string QuoteAmountCurrency { get; set; }
        

        public decimal TotalActiveTrades { get; set; }
        public decimal TotalNumberOfTrades { get; set; }
        public decimal AverageTradesPerMonth { get; set; }
        public decimal NumberOfProfitableTrades { get; set; }
        public decimal NumberOfLosingTrades { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalLoss { get; set; }
        public decimal PercentProfit { get; set; }
        public decimal LargestProfit { get; set; }
        public decimal LargestLoss { get; set; }

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
            return new StrategyInstance() {
                StrategyName = strategy,
                IsActive = true,
                IsBackTest = true,
                Pair = pair,
                PeriodSize = PeriodSize.FiveMinutes,
                StartingAmount = 1000m,
                BaseAmount = 1000m,
                BaseAmountCurrency = pair.SideIn(Side.Sell),
                QuoteAmount = 0,
                QuoteAmountCurrency =  pair.SideIn(Side.Buy),
                Reference = $"{strategy}_{pair}_{PeriodSize.FiveMinutes}_{DateTime.Now:yyyyMMdd}".ToLower()
            };
        }

        
    }
}