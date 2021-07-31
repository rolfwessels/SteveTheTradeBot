using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Tools;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.BackTesting
{

    public class BackTestRunner
    {
        private readonly CandleBuilder _candleBuilder;
        private readonly DynamicGraphs _dynamicGraphs;
        private readonly StrategyPicker _picker;
        private readonly StrategyInstanceStore _strategyInstanceStore;
        private readonly IBrokerApi _broker;
        private readonly IMessenger _messenger;
        private readonly StrategyRunner _strategyRunner;

        public BackTestRunner(DynamicGraphs dynamicGraphs, StrategyPicker picker , StrategyInstanceStore strategyInstanceStore, IBrokerApi broker, IMessenger messenger, StrategyRunner strategyRunner)
        {
            _candleBuilder = new CandleBuilder();
            _dynamicGraphs = dynamicGraphs;
            _picker = picker;
            _strategyInstanceStore = strategyInstanceStore;
            _broker = broker;
            _messenger = messenger;
            _strategyRunner = strategyRunner;
        }

        public async Task<StrategyInstance> Run(StrategyInstance instance,
            IEnumerable<TradeFeedCandle> enumerable, 
            CancellationToken cancellationToken)
        {
            
            await _dynamicGraphs.Clear(instance.Reference);
            var strategy = _picker.Get(instance.StrategyName);
            return await _strategyInstanceStore.EnsureUpdate(instance.Id, async si => {
                var context = await _strategyRunner.PopulateStrategyContext(si, DateTime.UtcNow);
                context.ByMinute.Clear();
                
                foreach (var trade in enumerable)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    context.ByMinute.Push(trade);
                    await _strategyRunner.Process(si, context, strategy);
                }
                await strategy.SellAll(context);
                await _strategyRunner.PostTransaction(si);
                return si;
            });
        }
    }

   

    public class StrategyContext : IParamsStoreSimple
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDynamicGraphs _dynamicGraphs;
        private readonly IParameterStore _parameterStore;

        public StrategyContext(IDynamicGraphs dynamicGraphs, StrategyInstance strategyInstance, IBrokerApi broker, IMessenger messenger, IParameterStore parameterStore)
        {
            _dynamicGraphs = dynamicGraphs;
            StrategyInstance = strategyInstance;
            _parameterStore = parameterStore;
            Messenger = messenger;
            ByMinute = new Recent<TradeFeedCandle>(1000);
            Broker = broker;

        }

        public Recent<TradeFeedCandle> ByMinute { get; }
        public StrategyInstance StrategyInstance { get; }

        public IBrokerApi Broker { get; }
        public IMessenger Messenger { get; }

        public Task Set(string key, string value)
        {
            StrategyInstance.Set(key, value);
            return Task.CompletedTask;
        }

        public Task<string> Get(string key, string defaultValue)
        {
            return Task.FromResult(StrategyInstance.Get(key, defaultValue));
        }

       

        public async Task PlotRunData(DateTime date, string label, decimal value)
        {
            _log.Debug($"StrategyContext:PlotRunData {StrategyInstance.Reference} {date}, {label}, {value}");
            await _dynamicGraphs.Plot(StrategyInstance.Reference, date, label, value);
        }

        public TradeFeedCandle LatestQuote()
        {
            return ByMinute.Last();
        }

        public StrategyTrade ActiveTrade()
        {
            return StrategyInstance.Trades.FirstOrDefault(x => x.IsActive);
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
