using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Components.BackTesting
{

    [Category("FullIntegration")]
    public class BackTestRunnerTests
    {
        private BackTestRunner _backTestRunner;

        [Test]
        [Timeout(240000)]
        public async Task Fast_recent_RSiStrategy()
        {
            // arrange
            Setup();
            var to = DateTime.Now;
            var from = to.AddDays(-6);

            var strategyInstances = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.Week).Select(x =>
                BuildBackTestResult(@from, to, t => new RSiStrategy(), x.Item2.CurrencyPair, x.Item1).Result);

            var table = strategyInstances.Select(x => new
                { x.Pair, x.PeriodSize, x.PercentMarketProfit, x.PercentProfit, x.AverageTradesPerMonth }).ToTable();
            Console.Out.WriteLine(table);

                // ╔════════╦════════════════╦═════════════════════╦═══════════════╦═══════════════════════╗                              
                // ║ Pair   ║ PeriodSize     ║ PercentMarketProfit ║ PercentProfit ║ AverageTradesPerMonth ║                              
                // ╠════════╬════════════════╬═════════════════════╬═══════════════╬═══════════════════════╣                              
                // ║ BTCZAR ║ OneMinute      ║ 16.421              ║ -8.030        ║ 11.105                ║                              
                // ║ BTCZAR ║ FiveMinutes    ║ 16.195              ║ 33.605        ║ 5.801                 ║                              
                // ║ BTCZAR ║ FifteenMinutes ║ 16.508              ║ 63.823        ║ 3.647                 ║                              
                // ║ BTCZAR ║ ThirtyMinutes  ║ 17.014              ║ -20.981       ║ 3.315                 ║                              
                // ║ BTCZAR ║ OneHour        ║ 17.953              ║ -23.472       ║ 2.984                 ║                              
                // ║ BTCZAR ║ Day            ║ 13.592              ║ 10.702        ║ 0.833                 ║                              
                // ║ ETHZAR ║ OneMinute      ║ 86.754              ║ 144.199       ║ 13.757                ║                              
                // ║ ETHZAR ║ FiveMinutes    ║ 85.317              ║ 65.962        ║ 8.122                 ║                              
                // ║ ETHZAR ║ FifteenMinutes ║ 85.952              ║ 44.759        ║ 5.801                 ║                              
                // ║ ETHZAR ║ ThirtyMinutes  ║ 85.380              ║ 10.046        ║ 4.476                 ║                              
                // ║ ETHZAR ║ OneHour        ║ 86.330              ║ 101.948       ║ 2.984                 ║                              
                // ║ ETHZAR ║ Day            ║ 65.079              ║ 0             ║ 0                     ║                              
                // ╚════════╩════════════════╩═════════════════════╩═══════════════╩═══════════════════════
        }

        [Test]
        [Timeout(240000)]
        public async Task Fast_RSiStrategy()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);

            var expected = 16m; //49
            await Test(@from, to, expected, t => new RSiStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }

        [Test]
        [Timeout(240000)]
        public async Task Fast_RSiMslStrategy()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 49; 
            await Test(@from, to, expected, t => new RSiMslStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }
        
        [Test]
        [Timeout(240000)]
        [Explicit]
        public async Task Run_GivenRSiMslStrategy_ShouldOver2YearsShouldMake400PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 470; // 209
            await Test(@from, to, expected, t => new RSiMslStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }


        [Test]
        [Timeout(240000)]
        public async Task Run_GivenRSiMslStrategy_ShouldOver1YearsShouldMake200PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2020-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 94; // 
            await Test(@from, to, expected , t => new RSiMslStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }
        
        private async Task Test(DateTime fromDate, DateTime to, decimal expected, Func<IBrokerApi,IStrategy> getStrategy, string currencyPair, PeriodSize size)
        {
            var backTestResult = await BuildBackTestResult(fromDate, to, getStrategy, currencyPair, size);
            // assert

            backTestResult.Print();

            backTestResult.PercentProfit.Should().BeGreaterThan(expected);
            backTestResult.PercentProfit.Should().BeGreaterThan(backTestResult.PercentMarketProfit);
        }

        private async Task<StrategyInstance> BuildBackTestResult(DateTime fromDate, DateTime to, Func<IBrokerApi, IStrategy> getStrategy, string currencyPair,
            PeriodSize size)
        {
            var factory = TestTradePersistenceFactory.RealDb();
            var tradeHistoryStore = new TradeHistoryStore(factory);
            var tradeFeedCandleStore = new TradeFeedCandlesStore(factory);
            var strategyInstanceStore = new StrategyInstanceStore(factory);
            var parameterStore = new ParameterStore(factory);
            var player = new HistoricalDataPlayer(tradeHistoryStore, tradeFeedCandleStore);

            var fakeBroker = new FakeBroker(Messenger.Default, tradeHistoryStore);
            var strategy = getStrategy(fakeBroker);
            var picker = new StrategyPicker().Add(strategy.Name, () => strategy);


            var strategyInstance = StrategyInstance.ForBackTest(strategy.Name, currencyPair);
            strategyInstance.PeriodSize = size;
            strategyInstance.Reference += $"{fromDate:yyMM}-{to:yyMM}";
            await strategyInstanceStore.RemoveByReference(strategyInstance.Reference);
            await strategyInstanceStore.Add(strategyInstance);

            var dynamicGraphs = new DynamicGraphs(factory);

            var strategyRunner = new StrategyRunner(picker, dynamicGraphs, strategyInstanceStore, fakeBroker,
                tradeFeedCandleStore, Messenger.Default, parameterStore);
            _backTestRunner = new BackTestRunner(dynamicGraphs, picker, strategyInstanceStore, fakeBroker, Messenger.Default,
                strategyRunner);
            var cancellationTokenSource = new CancellationTokenSource();

            var trades = player.ReadHistoricalData(currencyPair, fromDate.ToUniversalTime(), to.ToUniversalTime(),
                strategyInstance.PeriodSize, cancellationTokenSource.Token);
            // action

            var backTestResult = await _backTestRunner.Run(strategyInstance, trades, CancellationToken.None);
            return backTestResult;
        }

        #region Setup/Teardown

        public void Setup()
        {
          //  TestLoggingHelper.EnsureExists();
            
        }

        [TearDown]
        public void TearDown()
        {

        }

        #endregion
    }
}