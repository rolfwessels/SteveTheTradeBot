using System.Collections.Generic;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class BotInstance : BaseDalModelWithId
    {
        public string Reference { get; set; }
        public string BotName { get; set; }
        public bool IsActive { get; set; }
        public bool IsBackTest { get; set; }
        public Amount StartingAmount { get; set; }
        public List<Amount> CurrentHolding { get; set; }

        public decimal TotalActiveTrades { get; set; }
        public decimal TotalNumberOfTrades { get; set; }
        public decimal AverageTradesPerMonth { get; set; }
        public decimal NumberOfProfitableTrades { get; set; }
        public decimal NumberOfLosingTrades { get; set; }
        public Amount TotalProfit { get; set; }
        public Amount TotalLoss { get; set; }
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
    }
}