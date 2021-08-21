using System;
using System.Linq;
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
    }
}