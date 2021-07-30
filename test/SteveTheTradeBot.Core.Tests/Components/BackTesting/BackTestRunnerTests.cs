using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Components.BackTesting
{

    [Category("Integration")]
    public class BackTestRunnerTests
    {
        private BackTestRunner _backTestRunner;
        
        [Test]
        [Timeout(240000)]
        [Explicit]
        public async Task Run_GivenRSiBot_ShouldOver2YearsShouldMake400PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 470; // 209
            await Test(@from, to, expected, t => new RSiStrategy(), CurrencyPair.BTCZAR, PeriodSize.OneMinute);
        }

        [Test]
        [Timeout(240000)]
        public async Task Fast()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 49; // 209
            await Test(@from, to, expected, t => new RSiStrategy(), CurrencyPair.BTCZAR, PeriodSize.OneMinute);
        }

        [Test]
        [Timeout(240000)]
        public async Task Fast_Old()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 49; // 209
            await Test(@from, to, expected, t => new RSiStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }

        [Test]
        [Timeout(240000)]
        public async Task Run_GivenRSiBot_ShouldOver1YearsShouldMake200PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2020-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 94; // 
            await Test(@from, to, expected , t => new RSiStrategy(), CurrencyPair.BTCZAR, PeriodSize.OneMinute);
        }
        
        private async Task Test(DateTime fromDate, DateTime to, int expected, Func<IBrokerApi,IStrategy> getStrategy, string currencyPair, PeriodSize size)
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


            var strategyInstance = StrategyInstance.ForBackTest(strategy.Name, CurrencyPair.BTCZAR);
            strategyInstance.PeriodSize = size;
            strategyInstance.Reference += $"{fromDate:yyMM}-{to:yyMM}";
            await strategyInstanceStore.RemoveByReference(strategyInstance.Reference);
            await strategyInstanceStore.Add(strategyInstance);

            var dynamicGraphs = new DynamicGraphs(factory);
            
            var strategyRunner = new StrategyRunner(picker, dynamicGraphs, strategyInstanceStore, fakeBroker, tradeFeedCandleStore, Messenger.Default, parameterStore);
            _backTestRunner = new BackTestRunner(dynamicGraphs, picker, strategyInstanceStore, fakeBroker, Messenger.Default, strategyRunner);
            var cancellationTokenSource = new CancellationTokenSource();

            var trades = player.ReadHistoricalData(currencyPair, fromDate.ToUniversalTime(), to.ToUniversalTime(), strategyInstance.PeriodSize,cancellationTokenSource.Token);
            // action
            
            var backTestResult = await _backTestRunner.Run(strategyInstance, trades,  CancellationToken.None);
            // assert

            backTestResult.Print();

            backTestResult.PercentProfit.Should().BeGreaterThan(expected);
            backTestResult.PercentProfit.Should().BeGreaterThan(backTestResult.PercentMarketProfit);
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