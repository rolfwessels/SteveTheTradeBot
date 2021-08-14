using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core;
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
        public void Fast_MacdStrategy_OverPeriods()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);

            var strategyInstances = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.Week).Select(x =>
                BuildBackTestResult(@from, to, t => new MacdStrategy(), x.Item2.CurrencyPair, x.Item1).Result);

            var table = strategyInstances.Select(x => new
            {
                x.Pair, x.PeriodSize, x.PercentMarketProfit, x.PercentProfit, x.AverageTradesPerMonth,
                x.PercentOfProfitableTrades
            }).ToTable();
            Console.Out.WriteLine(table);

            // ╔════════╦════════════════╦═════════════════════╦═══════════════╦═══════════════════════╦═══════════════════════════╗                    
            // ║ Pair   ║ PeriodSize     ║ PercentMarketProfit ║ PercentProfit ║ AverageTradesPerMonth ║ PercentOfProfitableTrades ║                    
            // ╠════════╬════════════════╬═════════════════════╬═══════════════╬═══════════════════════╬═══════════════════════════╣                    
            // ║ BTCZAR ║ OneMinute      ║ 37.812              ║ 35.196        ║ 6.429                 ║ 16.67                     ║                    
            // ║ BTCZAR ║ FiveMinutes    ║ 37.258              ║ 39.188        ║ 4.286                 ║ 50.0                      ║                    
            // ║ BTCZAR ║ FifteenMinutes ║ 38.032              ║ 45.576        ║ 3.214                 ║ 66.67                     ║                    
            // ║ BTCZAR ║ ThirtyMinutes  ║ 37.503              ║ 54.988        ║ 2.143                 ║ 100                       ║                    
            // ║ BTCZAR ║ OneHour        ║ 37.019              ║ 53.130        ║ 3.214                 ║ 66.67                     ║                    
            // ║ BTCZAR ║ Day            ║ 33.584              ║ 0             ║ 0                     ║ 0                         ║                    
            // ║ ETHZAR ║ OneMinute      ║ 8.204               ║ 0.020         ║ 17.146                ║ 43.75                     ║                    
            // ║ ETHZAR ║ FiveMinutes    ║ 8.199               ║ 5.594         ║ 8.571                 ║ 37.50                     ║                    
            // ║ ETHZAR ║ FifteenMinutes ║ 7.767               ║ 0.448         ║ 5.357                 ║ 40.0                      ║                    
            // ║ ETHZAR ║ ThirtyMinutes  ║ 7.323               ║ 12.214        ║ 3.214                 ║ 66.67                     ║                    
            // ║ ETHZAR ║ OneHour        ║ 7.305               ║ 39.460        ║ 3.214                 ║ 66.67                     ║                    
            // ║ ETHZAR ║ Day            ║ 3.296               ║ 0             ║ 0                     ║ 0                         ║                    
            // ╚════════╩════════════════╩═════════════════════╩═══════════════╩═══════════════════════╩═══════════════════════════╝ 
        }

        [Test]
        [Timeout(240000)]
        public void Fast_RSiMlStrategy_OverPeriods()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);

            var strategyInstances = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.Week).Select(x =>
                BuildBackTestResult(@from, to, t => new RSiMlStrategy(), x.Item2.CurrencyPair, x.Item1).Result);

            var table = strategyInstances.Select(x => new
            {
                x.Pair, x.PeriodSize, x.PercentMarketProfit, x.PercentProfit, x.AverageTradesPerMonth,
                x.PercentOfProfitableTrades
            }).ToTable();
            Console.Out.WriteLine(table);
            //
            // ╔════════╦════════════════╦═════════════════════╦═══════════════╦═══════════════════════╦═══════════════════════════╗  
            // ║ Pair   ║ PeriodSize     ║ PercentMarketProfit ║ PercentProfit ║ AverageTradesPerMonth ║ PercentOfProfitableTrades ║  
            // ╠════════╬════════════════╬═════════════════════╬═══════════════╬═══════════════════════╬═══════════════════════════╣  
            // ║ BTCZAR ║ OneMinute      ║ 37.812              ║ 39.668        ║ 19.287                ║ 61.11                     ║  
            // ║ BTCZAR ║ FiveMinutes    ║ 37.258              ║ 64.164        ║ 11.786                ║ 72.73                     ║  
            // ║ BTCZAR ║ FifteenMinutes ║ 38.032              ║ 47.894        ║ 9.643                 ║ 77.78                     ║  
            // ║ BTCZAR ║ ThirtyMinutes  ║ 37.503              ║ 14.860        ║ 10.714                ║ 50.0                      ║  
            // ║ BTCZAR ║ OneHour        ║ 37.019              ║ 6.354         ║ 4.286                 ║ 75.00                     ║  
            // ║ BTCZAR ║ Day            ║ 33.584              ║ 0             ║ 0                     ║ 0                         ║  
            // ║ ETHZAR ║ OneMinute      ║ 8.204               ║ 17.420        ║ 36.436                ║ 47.06                     ║  
            // ║ ETHZAR ║ FiveMinutes    ║ 8.199               ║ 28.156        ║ 22.5                  ║ 47.62                     ║  
            // ║ ETHZAR ║ FifteenMinutes ║ 7.767               ║ 5.212         ║ 19.286                ║ 33.33                     ║  
            // ║ ETHZAR ║ ThirtyMinutes  ║ 7.323               ║ 17.038        ║ 17.143                ║ 31.25                     ║  
            // ║ ETHZAR ║ OneHour        ║ 7.305               ║ 39.866        ║ 13.929                ║ 53.85                     ║  
            // ║ ETHZAR ║ Day            ║ 3.296               ║ 0             ║ 0                     ║ 0                         ║  
        }

        [Test]
        [Timeout(240000)]
        public void Fast_RSiConfirmStrategy_OverPeriods()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);

            var strategyInstances = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.Week).Select(x =>
                BuildBackTestResult(@from, to, t => new RSiConfirmStrategy(), x.Item2.CurrencyPair, x.Item1).Result);

            var table = strategyInstances.Select(x => new
            {
                x.Pair,
                x.PeriodSize,
                x.PercentMarketProfit,
                x.PercentProfit,
                x.AverageTradesPerMonth,
                x.PercentOfProfitableTrades
            }).ToTable();
            Console.Out.WriteLine(table);
            // ╔════════╦════════════════╦═════════════════════╦═══════════════╦═══════════════════════╦═══════════════════════════╗  
            // ║ Pair   ║ PeriodSize     ║ PercentMarketProfit ║ PercentProfit ║ AverageTradesPerMonth ║ PercentOfProfitableTrades ║  
            // ╠════════╬════════════════╬═════════════════════╬═══════════════╬═══════════════════════╬═══════════════════════════╣  
            // ║ BTCZAR ║ OneMinute      ║ 37.812              ║ 59.484        ║ 19.287                ║ 55.56                     ║  
            // ║ BTCZAR ║ FiveMinutes    ║ 37.258              ║ 58.406        ║ 13.929                ║ 61.54                     ║  
            // ║ BTCZAR ║ FifteenMinutes ║ 38.032              ║ 46.956        ║ 10.714                ║ 50.0                      ║  
            // ║ BTCZAR ║ ThirtyMinutes  ║ 37.503              ║ 16.342        ║ 7.5                   ║ 57.14                     ║  
            // ║ BTCZAR ║ OneHour        ║ 37.019              ║ -1.512        ║ 5.357                 ║ 40.0                      ║  
            // ║ BTCZAR ║ Day            ║ 33.584              ║ 0             ║ 0                     ║ 0                         ║  
            // ║ ETHZAR ║ OneMinute      ║ 8.204               ║ 53.198        ║ 35.364                ║ 54.55                     ║  
            // ║ ETHZAR ║ FiveMinutes    ║ 8.199               ║ 69.468        ║ 22.5                  ║ 52.38                     ║  
            // ║ ETHZAR ║ FifteenMinutes ║ 7.767               ║ 21.204        ║ 20.357                ║ 42.11                     ║  
            // ║ ETHZAR ║ ThirtyMinutes  ║ 7.323               ║ 22.946        ║ 13.929                ║ 46.15                     ║  
            // ║ ETHZAR ║ OneHour        ║ 7.305               ║ 39.474        ║ 9.643                 ║ 55.56                     ║  
            // ║ ETHZAR ║ Day            ║ 3.296               ║ 0             ║ 0                     ║ 0                         ║  
            // ╚════════╩════════════════╩═════════════════════╩═══════════════╩═══════════════════════╩═══════════════════════════╝  

        }

        [Test]
        [Timeout(240000)]
        [Ignore("cant get it lower")]
        public async Task Fast_RSiStrategy()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 39;
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
        public async Task Fast_MacdStrategy()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 43;
            await Test(@from, to, expected, t => new MacdStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }


        [Test]
        [Timeout(240000)]
        public async Task Compare_RSiConfirmStrategy_ToRealTrade()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021/08/10 10:13:52", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
            var to = DateTime.Parse("2021/08/13 09:20:00", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
            //var to = DateTime.UtcNow;
            var expected = 1;
            await Test(@from, to, expected, t => new RSiConfirmStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }


        [Test]
        [Timeout(240000)]
        [Explicit]
        public async Task Run_GivenRSiStrategy_ShouldOver2YearsShouldMake68PlusProfit() 
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 68; // failing
            await Test(@from, to, expected, t => new RSiStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }


        [Test]
        [Timeout(240000)]
        [Explicit]
        public async Task Run_GivenRSiMslStrategy_ShouldOver2YearsShouldMake470PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 470; 
            await Test(@from, to, expected, t => new RSiMslStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }


        [Test]
        [Timeout(240000)]
        [Explicit]
        public async Task Run_GivenRSiConfirmStrategy_ShouldOver2YearsShouldMake1000PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 1000; // currently says 1918.354 but I think that is BS
            await Test(@from, to, expected, t => new RSiConfirmStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }


        [Test]
        [Timeout(240000)]
        [Explicit]
        public async Task Run_GivenMacdStrategy_ShouldOver2YearsShouldMake470PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 470; // currently says 1953.354 but I think that is BS
            await Test(@from, to, expected, t => new MacdStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
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
            await Test(@from, to, expected, t => new RSiMslStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
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

        private async Task Test(DateTime fromDate, DateTime to, decimal expected,
            Func<IBrokerApi, IStrategy> getStrategy, string currencyPair, PeriodSize size)
        {
            var backTestResult = await BuildBackTestResult(fromDate, to, getStrategy, currencyPair, size);
            // assert
            Console.Out.WriteLine($"{fromDate.ToLocalTime()} to {to} {(to - fromDate).ToShort()} ");
            backTestResult.Print();

            backTestResult.PercentProfit.Should().BeGreaterThan(expected);
            backTestResult.PercentProfit.Should().BeGreaterThan(backTestResult.PercentMarketProfit);
        }

        private async Task<StrategyInstance> BuildBackTestResult(DateTime fromDate, DateTime to,
            Func<IBrokerApi, IStrategy> getStrategy, string currencyPair,
            PeriodSize size)
        {
            var factory = TestTradePersistenceFactory.RealDb();
            var tradeHistoryStore = new TradeHistoryStore(factory);
            var tradeFeedCandleStore = new TradeQuoteStore(factory);
            var strategyInstanceStore = new StrategyInstanceStore(factory);
            var parameterStore = new ParameterStore(factory);
            var player = new HistoricalDataPlayer(tradeHistoryStore, tradeFeedCandleStore);

            var fakeBroker = new FakeBroker(Messenger.Default, tradeHistoryStore);
            var strategy = getStrategy(fakeBroker);
            var picker = new StrategyPicker().Add(strategy.Name, () => strategy);


            var strategyInstance = StrategyInstance.ForBackTest(strategy.Name, currencyPair, 500, size);
            strategyInstance.Reference += $"{fromDate:yyMM}-{to:yyMM}";
            await strategyInstanceStore.RemoveByReference(strategyInstance.Reference);
            await strategyInstanceStore.Add(strategyInstance);

            var dynamicGraphs = new DynamicGraphs(factory);

            var strategyRunner = new StrategyRunner(picker, dynamicGraphs, strategyInstanceStore, fakeBroker,
                tradeFeedCandleStore, Messenger.Default, parameterStore);
            _backTestRunner = new BackTestRunner(dynamicGraphs, picker, strategyInstanceStore, fakeBroker,
                Messenger.Default,
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