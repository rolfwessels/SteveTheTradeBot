using System;
using System.Linq;
using System.Reflection;
using Serilog;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Utils
{
    public static class StrategyInstanceHelper
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        public static void Recalculate(this StrategyInstance strategyInstance)
        {
            try
            {
                strategyInstance.TotalFee = strategyInstance.Trades.Sum(x=>x.FeeAmount);
                strategyInstance.TotalActiveTrades = strategyInstance.Trades.Count(x => x.IsActive);
                strategyInstance.TotalNumberOfTrades = strategyInstance.Trades.Count;

                strategyInstance.AverageTradesPerMonth = strategyInstance.TotalNumberOfTrades > 1? Math.Round(strategyInstance.Trades.Count / ((strategyInstance.LastDate - strategyInstance.FirstStart).TotalDays / 30), 3):0;
                strategyInstance.NumberOfProfitableTrades = strategyInstance.Trades.Count(x => !x.IsActive && x.IsProfit());
                strategyInstance.NumberOfLosingTrades = strategyInstance.Trades.Count(x => !x.IsActive && !x.IsProfit());
                strategyInstance.PercentOfProfitableTrades = strategyInstance.TotalNumberOfTrades==0?0:Math.Round(strategyInstance.NumberOfProfitableTrades / strategyInstance.TotalNumberOfTrades * 100,2);
                strategyInstance.TotalProfit = strategyInstance.Trades.Where(x => !x.IsActive && x.IsProfit()).Sum(x => x.PriceDifference());
                strategyInstance.TotalLoss = strategyInstance.Trades.Where(x => !x.IsActive && !x.IsProfit()).Sum(x => x.PriceDifference());
                strategyInstance.PercentProfit = TradeUtils.MovementPercent(strategyInstance.QuoteAmount, strategyInstance.InvestmentAmount);
                strategyInstance.LargestProfit = strategyInstance.TotalNumberOfTrades == 0 ? 0 : strategyInstance.Trades.Where(x => !x.IsActive && x.IsProfit())
                    .Select(x => x.PriceDifference()).DefaultIfEmpty().Max();
                strategyInstance.LargestLoss = strategyInstance.TotalNumberOfTrades == 0 ? 0 : strategyInstance.Trades.Where(x => !x.IsActive && !x.IsProfit())
                    .Select(x => x.PriceDifference()).DefaultIfEmpty().Max();
                strategyInstance.PercentMarketProfit =
                    TradeUtils.MovementPercent(strategyInstance.LastClose, strategyInstance.FirstClose);
                strategyInstance.AverageTimeInMarket = TimeSpan.FromHours(strategyInstance.Trades.Where(x => !x.IsActive)
                    .Select(x => (x.EndDate ?? DateTime.Now) - x.StartDate)
                    .Sum(x => x.TotalHours));
            }
            catch (Exception e)
            {
                _log.Warning(e,e.Message);
            }
        }
    }
}