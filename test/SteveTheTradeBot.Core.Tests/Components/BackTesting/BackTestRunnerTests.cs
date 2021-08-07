﻿using System;
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
        public void Fast_recent_RSiConfirmStrategy()
        {
            // arrange
            Setup();
            var to = DateTime.Now;
            var from = to.AddMonths(-3);

            var strategyInstances = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.Week).Select(x =>
                BuildBackTestResult(@from, to, t => new RSiConfirmStrategy(), x.Item2.CurrencyPair, x.Item1).Result);

            var table = strategyInstances.Select(x => new
                { x.Pair, x.PeriodSize, x.PercentMarketProfit, x.PercentProfit, x.AverageTradesPerMonth }).ToTable();
            Console.Out.WriteLine(table);

                // ╔════════╦════════════════╦═════════════════════╦═══════════════╦═══════════════════════╗                              
                // ║ Pair   ║ PeriodSize     ║ PercentMarketProfit ║ PercentProfit ║ AverageTradesPerMonth ║                              
                // ╠════════╬════════════════╬═════════════════════╬═══════════════╬═══════════════════════╣                              
                // ║ BTCZAR ║ OneMinute      ║ -21.802             ║ -32.950       ║ 16.321                ║                              
                // ║ BTCZAR ║ FiveMinutes    ║ -22.048             ║ -0.198        ║ 7.508                 ║                              
                // ║ BTCZAR ║ FifteenMinutes ║ -21.906             ║ -3.150        ║ 4.57                  ║                              
                // ║ BTCZAR ║ ThirtyMinutes  ║ -22.046             ║ -4.763        ║ 3.265                 ║                              
                // ║ BTCZAR ║ OneHour        ║ -23.042             ║ 3.534         ║ 3.267                 ║                              
                // ║ BTCZAR ║ Day            ║ -23.769             ║ 0.299         ║ 0                     ║                              
                // ║ ETHZAR ║ OneMinute      ║ -9.730              ║ 32.165        ║ 16.648                ║                              
                // ║ ETHZAR ║ FiveMinutes    ║ -9.732              ║ 30.175        ║ 11.425                ║                              
                // ║ ETHZAR ║ FifteenMinutes ║ -9.959              ║ 61.464        ║ 6.856                 ║                              
                // ║ ETHZAR ║ ThirtyMinutes  ║ -10.712             ║ 21.426        ║ 6.204                 ║                              
                // ║ ETHZAR ║ OneHour        ║ -10.269             ║ 6.677         ║ 2.94                  ║                              
                // ║ ETHZAR ║ Day            ║ -18.695             ║ 0             ║ 0                     ║                              
                // ╚════════╩════════════════╩═════════════════════╩═══════════════╩═══════════════════════╝ 
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
        public async Task Fast_RSiConfirmStrategy()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 51;
            await Test(@from, to, expected, t => new RSiConfirmStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
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
        [Explicit]
        public async Task Run_GivenRSiConfirmStrategy_ShouldOver2YearsShouldMake400PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 470; // 209
            await Test(@from, to, expected, t => new RSiConfirmStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
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

        [Test]
        [Timeout(240000)]
        public async Task Run_GivenRSiConfirmStrategy_ShouldOver1YearsShouldMake200PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2020-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 294; // 
            await Test(@from, to, expected, t => new RSiConfirmStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
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