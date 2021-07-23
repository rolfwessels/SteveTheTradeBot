using System;
using System.Collections.Generic;
using System.Linq;
using BetterConsoleTables;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Utils
{
    public static class TradeUtils
    {
        public static decimal MovementPercent(decimal currentValue, decimal fromValue, int decimals = 3)
        {
            if (fromValue == 0) fromValue = 0.00001m;
            return Math.Round((currentValue - fromValue) / fromValue * 100, decimals);
        }

        public static ConsoleTables ToTable<T>(IEnumerable<T> enumerable)
        {
            var table = new Table().From(enumerable.ToList()).With(x => x.Config = TableConfiguration.UnicodeAlt());
            return new ConsoleTables(table);
        }


        public static void Recalculate(StrategyInstance strategyInstance)
        {
            strategyInstance.TotalFee = strategyInstance.Trades.Where(x => x.IsActive).Sum(x=>x.FeeAmount);
            strategyInstance.TotalActiveTrades = strategyInstance.Trades.Count(x => x.IsActive);
            strategyInstance.TotalNumberOfTrades = strategyInstance.Trades.Count;
            strategyInstance.AverageTradesPerMonth = Math.Round(strategyInstance.Trades.Count / (strategyInstance.LastDate - strategyInstance.FirstStart).TotalDays / 30, 3);
            strategyInstance.NumberOfProfitableTrades = strategyInstance.Trades.Count(x => !x.IsActive && x.Profit > 0);
            strategyInstance.NumberOfLosingTrades = strategyInstance.Trades.Count(x => !x.IsActive && x.Profit <= 0);
            strategyInstance.PercentOfProfitableTrades = Math.Round(strategyInstance.NumberOfProfitableTrades / strategyInstance.TotalNumberOfTrades * 100,2);
            strategyInstance.TotalProfit = strategyInstance.Trades.Where(x => !x.IsActive && x.Profit > 0).Sum(x => x.SellValue - x.BuyValue);
            strategyInstance.TotalLoss = strategyInstance.Trades.Where(x => !x.IsActive && x.Profit < 0).Sum(x => x.BuyValue - x.SellValue);
            strategyInstance.PercentProfit = MovementPercent(strategyInstance.BaseAmount, strategyInstance.InvestmentAmount);
            strategyInstance.LargestProfit = strategyInstance.Trades.Where(x => !x.IsActive && x.Profit > 0)
                .Select(x => x.SellValue - x.BuyValue).Max();
            strategyInstance.LargestLoss = strategyInstance.Trades.Where(x => !x.IsActive && x.Profit < 0)
                .Select(x => x.BuyValue - x.SellValue).Max();
            strategyInstance.PercentMarketProfit =
                MovementPercent(strategyInstance.LastClose, strategyInstance.FirstClose);
            strategyInstance.AverageTimeInMarket = TimeSpan.FromHours(strategyInstance.Trades.Where(x => !x.IsActive)
                .Select(x => (x.EndDate ?? DateTime.Now) - x.StartDate)
                .Sum(x => x.TotalHours));
        }
    }
}