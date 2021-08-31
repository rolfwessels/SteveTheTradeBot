using System;
using System.Collections.Generic;
using System.Linq;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Utils
{
    public class ProfitAndLossCalculator
    {
        public IEnumerable<ProfitLoss> GetDailyProfitAndLosses(StrategyInstance strategyInstance)
        {

            return strategyInstance.Trades.Select(x => new ProfitLoss() {Date = x.StartDate.Date, Profit = x.Profit, Return = x.TheReturn});
        }
    }

    public class ProfitLoss
    {
        public DateTime Date { get; set; }
        public decimal Profit { get; set; }
        public decimal Return { get; set; }
    }
}