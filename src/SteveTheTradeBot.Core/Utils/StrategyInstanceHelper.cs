using System;
using System.Linq;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Utils
{
    public static class StrategyInstanceHelper
    {
        public static void Recalculate(this StrategyInstance strategyInstance)
        {
            strategyInstance.TotalFee = strategyInstance.Trades.Where(x => x.IsActive).Sum(x=>x.FeeAmount);
            strategyInstance.TotalActiveTrades = strategyInstance.Trades.Count(x => x.IsActive);
            strategyInstance.TotalNumberOfTrades = strategyInstance.Trades.Count;

            strategyInstance.AverageTradesPerMonth = Math.Round(strategyInstance.Trades.Count / ((strategyInstance.LastDate - strategyInstance.FirstStart).TotalDays / 30), 3);
            strategyInstance.NumberOfProfitableTrades = strategyInstance.Trades.Count(x => !x.IsActive && x.Profit > 0);
            strategyInstance.NumberOfLosingTrades = strategyInstance.Trades.Count(x => !x.IsActive && x.Profit <= 0);
            strategyInstance.PercentOfProfitableTrades = strategyInstance.TotalNumberOfTrades==0?0:Math.Round(strategyInstance.NumberOfProfitableTrades / strategyInstance.TotalNumberOfTrades * 100,2);
            strategyInstance.TotalProfit = strategyInstance.Trades.Where(x => !x.IsActive && x.Profit > 0).Sum(x => x.SellValue - x.BuyValue);
            strategyInstance.TotalLoss = strategyInstance.Trades.Where(x => !x.IsActive && x.Profit < 0).Sum(x => x.BuyValue - x.SellValue);
            strategyInstance.PercentProfit = TradeUtils.MovementPercent(strategyInstance.BaseAmount, strategyInstance.InvestmentAmount);
            strategyInstance.LargestProfit = strategyInstance.TotalNumberOfTrades == 0 ? 0 : strategyInstance.Trades.Where(x => !x.IsActive && x.Profit > 0)
                .Select(x => x.SellValue - x.BuyValue).DefaultIfEmpty().Max();
            strategyInstance.LargestLoss = strategyInstance.TotalNumberOfTrades == 0 ? 0 : strategyInstance.Trades.Where(x => !x.IsActive && x.Profit < 0)
                .Select(x => x.BuyValue - x.SellValue).DefaultIfEmpty().Max();
            strategyInstance.PercentMarketProfit =
                TradeUtils.MovementPercent(strategyInstance.LastClose, strategyInstance.FirstClose);
            strategyInstance.AverageTimeInMarket = TimeSpan.FromHours(strategyInstance.Trades.Where(x => !x.IsActive)
                .Select(x => (x.EndDate ?? DateTime.Now) - x.StartDate)
                .Sum(x => x.TotalHours));
        }
    }
}