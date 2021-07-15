using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Tools;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public class BackTestRunner
    {
        private readonly CandleBuilder _candleBuilder;
        private readonly DynamicGraphs _dynamicGraphs;

        public BackTestRunner(DynamicGraphs dynamicGraphs)
        {
            _candleBuilder = new CandleBuilder();
            _dynamicGraphs = dynamicGraphs;
        }

        public async Task<BackTestResult> Run(IEnumerable<IQuote> enumerable, RSiBot.IBot bot,
            CancellationToken cancellationToken)
        {
            var runName = $"BT-{bot.Name}-{DateTime.Now:yyMMdd}";
            await _dynamicGraphs.Clear(runName);
            var botData = new BotData(_dynamicGraphs, 1000, runName);
            foreach (var trade in enumerable)
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (botData.BackTestResult.MarketOpenAt == 0) botData.BackTestResult.MarketOpenAt = trade.Close;
                botData.ByMinute.Push(trade);
                await bot.DataReceived(botData);
                botData.BackTestResult.MarketClosedAt = trade.Close;
            }
            await _dynamicGraphs.Flush();
            return botData.BackTestResult;
        }
        public class BotData
        {
            private readonly DynamicGraphs _dynamicGraphs;
            private readonly string _runName;

            public BotData(DynamicGraphs dynamicGraphs, int startingAmount, string runName)
            {
                _dynamicGraphs = dynamicGraphs;
                _runName = runName;
                BackTestResult = new BackTestResult {StartingAmount = startingAmount };
                ByMinute = new Recent<IQuote>(1000);
            }

            public Recent<IQuote> ByMinute { get; }
            public BackTestResult BackTestResult { get; set; }

            public async Task PlotRunData(DateTime date, string label, decimal value)
            {
                await _dynamicGraphs.Plot(_runName, date, label, value);
            }
        }
    }
}
/*
 * // Output the results
 http://www.modulusfe.com/products/trading-system-developer-components/back-test-lib/

Console .WriteLine( "Total number of trades: {0:G}" , results.TotalNumberOfTrades);

Console .WriteLine( "Average number of trades per month: {0:G}" , results.AverageTradesPerMonth);

Console .WriteLine( "Total number of profitable trades: {0:G}" , results.NumberOfProfitableTrades);

Console .WriteLine( "Total number of losing trades: {0:G}" , results.NumberOfLosingTrades);

Console .WriteLine( "Total profit: {0:C}" , results.TotalProfit);

Console .WriteLine( "Total loss: {0:C}" , results.TotalLoss);

Console .WriteLine( "Percent profitable trades: {0:P}" , results.PercentProfit);

Console .WriteLine( "Percent profitable trades: {0:P}" , results.PercentProfit);

Console .WriteLine( "Largest profit: {0:C}" , results.LargestProfit);

Console .WriteLine( "Largest loss: {0:C}" , results.LargestLoss);

Console .WriteLine( "Maximum drawdown: {0:C}" , results.MaximumDrawDown);

Console .WriteLine( "Maximum drawdown Monte Carlo: {0:C}" , results.MaximumDrawDownMonteCarlo);

Console .WriteLine( "Standard deviation: {0:G}" , results.StandardDeviation);

Console .WriteLine( "Standard deviation annualized: {0:G}" , results.StandardDeviationAnnualized);

Console .WriteLine( "Downside deviation (MAR = 10%): {0:G}" , results.DownsideDeviationMar10);

Console .WriteLine( "Value Added Monthly Index (VAMI): {0:G}" , results.ValueAddedMonthlyIndex);

Console .WriteLine( "Sharpe ratio: {0:G}" , results.SharpeRatio);

Console .WriteLine( "Sortino ratio: {0:G}" , results.SortinoRatioMAR5);

Console .WriteLine( "Annualized Sortino ratio: {0:G}" , results.AnnualizedSortinoRatioMAR5);

Console .WriteLine( "Sterling ratio: {0:G}" , results.SterlingRatioMAR5);

Console .WriteLine( "Calmar ratio: {0:G}" , results.CalmarRatio);

Console .WriteLine( "Risk to reward ratio: {0:P}" , results.RiskRewardRatio);
 */
