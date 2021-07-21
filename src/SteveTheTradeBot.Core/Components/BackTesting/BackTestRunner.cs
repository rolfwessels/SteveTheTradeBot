using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Bots;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Tools;
using SteveTheTradeBot.Dal.Models.Trades;

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

        public async Task<BackTestResult> Run(IEnumerable<TradeFeedCandle> enumerable, IBot bot,
            CancellationToken cancellationToken, string currencyPair)
        {
            var runName = $"BT-{bot.Name}-{currencyPair}-{DateTime.Now:yyMMdd}";
            await _dynamicGraphs.Clear(runName);
            var botData = new BotData(_dynamicGraphs, 1000, runName, currencyPair);
            foreach (var trade in enumerable)
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (botData.BackTestResult.MarketOpenAt == 0) botData.BackTestResult.MarketOpenAt = trade.Close;
                botData.ByMinute.Push(trade);
                await bot.DataReceived(botData);
                botData.BackTestResult.MarketClosedAt = trade.Close;
                
            }
            await _dynamicGraphs.Flush();
            await bot.SellAll(botData);
            return botData.BackTestResult;
        }

        public class BotData
        {
            private readonly IDynamicGraphs _dynamicGraphs;
            private readonly string _runName;

            public BotData(IDynamicGraphs dynamicGraphs, int startingAmount, string runName, string currencyPair)
            {
                _dynamicGraphs = dynamicGraphs;
                _runName = runName;
                BackTestResult = new BackTestResult {StartingAmount = startingAmount , CurrencyPair = currencyPair};
                ByMinute = new Recent<TradeFeedCandle>(1000);
            }

            public Recent<TradeFeedCandle> ByMinute { get; }
            public BackTestResult BackTestResult { get; set; }
            

            public async Task PlotRunData(DateTime date, string label, decimal value)
            {
                await _dynamicGraphs.Plot(_runName, date, label, value);
            }

            public TradeFeedCandle LatestQuote()
            {
                return ByMinute.Last();
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
