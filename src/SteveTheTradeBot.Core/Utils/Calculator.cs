using System;
using System.Linq;
using Bumbershoot.Utilities.Helpers;
using MathNet.Numerics.Statistics;

namespace SteveTheTradeBot.Core.Utils
{
    public static class Calculator
    {
        public static decimal SharpeRatioForYear(decimal[] yearData, decimal riskFreeReturn = 2m)
        {
            var expectedPortfolioReturn = yearData.Average();
            var excessReturns = yearData.Select(x=> (double)(x - riskFreeReturn));
            var standardDeviation = excessReturns.StandardDeviation();
            return Math.Round((expectedPortfolioReturn - riskFreeReturn)/ (decimal) standardDeviation,2);
        }

        public static decimal SharpeRatioOverPeriods(decimal[] monthData, int period, decimal riskFreeReturn = 5m)
        {
            var expectedPortfolioReturn = monthData.Average();
            var excessReturns = monthData.Select(x => (double)(x - (riskFreeReturn/period)));
            var standardDeviation = excessReturns.PopulationStandardDeviation();
            var portfolioReturn = (expectedPortfolioReturn - (riskFreeReturn)) / (decimal)(standardDeviation);
            return Math.Round(portfolioReturn * (decimal) Math.Sqrt(period), 3);
        }

    }
}