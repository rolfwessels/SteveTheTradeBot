using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.BackTesting
{

    public class BackTestRunner
    {
        private readonly DynamicGraphs _dynamicGraphs;
        private readonly StrategyPicker _picker;
        private readonly StrategyInstanceStore _strategyInstanceStore;
        private readonly StrategyRunner _strategyRunner;

        public BackTestRunner(DynamicGraphs dynamicGraphs, StrategyPicker picker , StrategyInstanceStore strategyInstanceStore, StrategyRunner strategyRunner)
        {
            _dynamicGraphs = dynamicGraphs;
            _picker = picker;
            _strategyInstanceStore = strategyInstanceStore;
            _strategyRunner = strategyRunner;
        }

        public async Task<StrategyInstance> Run(StrategyInstance instance,
            IEnumerable<TradeQuote> periodQuotes, 
            List<TradeQuote> dayQuotes, 
            CancellationToken cancellationToken)
        {   
            await _dynamicGraphs.Clear(instance.Reference);
            var strategy = _picker.Get(instance.StrategyName);
            var dayQuoteIndex = 0;
            return await _strategyInstanceStore.EnsureUpdate(instance.Id, async si => {
                var context = await _strategyRunner.PopulateStrategyContext(si, DateTime.UtcNow);
                context.Quotes.Clear();
                
                foreach (var trade in periodQuotes)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    context.Quotes.Push(trade);
                    var dayQuote = dayQuotes[dayQuoteIndex];
                    
                    while (dayQuote.Date.AddDays(1) <= trade.Date)
                    {
                        context.DayQuotes.Push(dayQuote);
                        var i = ++dayQuoteIndex;
                        if (i >= 0 && dayQuotes.Count > i) dayQuote = dayQuotes[i];
                        else break;
                    }

                   
                    

                    await _strategyRunner.Process(si, context, strategy);
                }
                await strategy.SellAll(context);
                await _strategyRunner.PostTransaction(si);
                return si;
            });
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
