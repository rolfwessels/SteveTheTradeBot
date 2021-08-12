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
        public void Fast_recent_RSiConfirmStrategy()
        {
            // arrange
            Setup();
            var to = DateTime.Now;
            var from = to.AddMonths(-3);

            var strategyInstances = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.Week).Select(x =>
                BuildBackTestResult(@from, to, t => new RSiConfirmStrategy(), x.Item2.CurrencyPair, x.Item1).Result);

            var table = strategyInstances.Select(x => new
                { x.Pair, x.PeriodSize, x.PercentMarketProfit, x.PercentProfit, x.AverageTradesPerMonth , x.PercentOfProfitableTrades }).ToTable();
            Console.Out.WriteLine(table);

                // ╔════════╦════════════════╦═════════════════════╦═══════════════╦═══════════════════════╗                              
                // ║ Pair   ║ PeriodSize     ║ PercentMarketProfit ║ PercentProfit ║ AverageTradesPerMonth ║                              
                // ╠════════╬════════════════╬═════════════════════╬═══════════════╬═══════════════════════╣                              
                // ║ BTCZAR ║ OneMinute      ║ -22.052             ║ -3.805        ║ 23.504                ║                              
                // ║ BTCZAR ║ FiveMinutes    ║ -21.958             ║ 48.299        ║ 19.914                ║                              
                // ║ BTCZAR ║ FifteenMinutes ║ -22.043             ║ 66.334        ║ 15.345                ║                              
                // ║ BTCZAR ║ ThirtyMinutes  ║ -22.046             ║ 76.421        ║ 11.755                ║                              
                // ║ BTCZAR ║ OneHour        ║ -23.042             ║ 72.472        ║ 7.84                  ║                              
                // ║ BTCZAR ║ Day            ║ -23.769             ║ 0.299         ║ 0                     ║                              
                // ║ ETHZAR ║ OneMinute      ║ -9.816              ║ 68.933        ║ 35.909                ║                              
                // ║ ETHZAR ║ FiveMinutes    ║ -9.991              ║ 153.104       ║ 25.79                 ║                              
                // ║ ETHZAR ║ FifteenMinutes ║ -10.165             ║ 208.410       ║ 19.263                ║                              
                // ║ ETHZAR ║ ThirtyMinutes  ║ -10.712             ║ 265.288       ║ 14.367                ║                              
                // ║ ETHZAR ║ OneHour        ║ -10.269             ║ 63.173        ║ 8.167                 ║                              
                // ║ ETHZAR ║ Day            ║ -18.695             ║ 0             ║ 0                     ║                              
                // ╚════════╩════════════════╩═════════════════════╩═══════════════╩═══════════════════════╝ 
        }

        [Test]
        [Timeout(240000)]
        public void Fast_recent_RSiMlStrategy()
        {
            // arrange
            Setup();
            var to = DateTime.Now;
            var from = to.AddMonths(-3);

            var strategyInstances = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.Week).Select(x =>
                BuildBackTestResult(@from, to, t => new RSiMlStrategy(),  x.Item2.CurrencyPair, x.Item1).Result);

            var table = strategyInstances.Select(x => new
            { x.Pair, x.PeriodSize, x.PercentMarketProfit, x.PercentProfit, x.AverageTradesPerMonth, x.PercentOfProfitableTrades }).ToTable();
            Console.Out.WriteLine(table);

            // ╔════════╦════════════════╦═════════════════════╦═══════════════╦═══════════════════════╗                              
            // ║ Pair   ║ PeriodSize     ║ PercentMarketProfit ║ PercentProfit ║ AverageTradesPerMonth ║                              
            // ╠════════╬════════════════╬═════════════════════╬═══════════════╬═══════════════════════╣                              
            // ║ BTCZAR ║ OneMinute      ║ -22.052             ║ -3.805        ║ 23.504                ║                              
            // ║ BTCZAR ║ FiveMinutes    ║ -21.958             ║ 48.299        ║ 19.914                ║                              
            // ║ BTCZAR ║ FifteenMinutes ║ -22.043             ║ 66.334        ║ 15.345                ║                              
            // ║ BTCZAR ║ ThirtyMinutes  ║ -22.046             ║ 76.421        ║ 11.755                ║                              
            // ║ BTCZAR ║ OneHour        ║ -23.042             ║ 72.472        ║ 7.84                  ║                              
            // ║ BTCZAR ║ Day            ║ -23.769             ║ 0.299         ║ 0                     ║                              
            // ║ ETHZAR ║ OneMinute      ║ -9.816              ║ 68.933        ║ 35.909                ║                              
            // ║ ETHZAR ║ FiveMinutes    ║ -9.991              ║ 153.104       ║ 25.79                 ║                              
            // ║ ETHZAR ║ FifteenMinutes ║ -10.165             ║ 208.410       ║ 19.263                ║                              
            // ║ ETHZAR ║ ThirtyMinutes  ║ -10.712             ║ 265.288       ║ 14.367                ║                              
            // ║ ETHZAR ║ OneHour        ║ -10.269             ║ 63.173        ║ 8.167                 ║                              
            // ║ ETHZAR ║ Day            ║ -18.695             ║ 0             ║ 0                     ║                              
            // ╚════════╩════════════════╩═════════════════════╩═══════════════╩═══════════════════════╝ 
        }


        [Test]
        [Timeout(240000)]
        public async Task Fast_RSiStrategy()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 49;
            await Test(@from, to, expected, t => new RSiStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
                // ╔═════════════════════╦══════════╦═══════════════╦══════════╦═══════════╦════════╦═══════════════╗                     
                // ║ StartDate           ║ BuyValue ║ Quantity      ║ BuyPrice ║ SellPrice ║ Profit ║ FeeAmount     ║                     
                // ╠═════════════════════╬══════════╬═══════════════╬══════════╬═══════════╬════════╬═══════════════╣                     
                // ║ 2021/02/01 13:50:00 ║ 1000     ║ 0.00190285524 ║ 525001   ║ 598001    ║ 13.677 ║ 2.14000090476 ║                     
                // ║ 2021/02/09 09:30:00 ║ 1136.77  ║ 0.00159993846 ║ 709800   ║ 688001    ║ -3.265 ║ 2.23677309200 ║                     
                // ║ 2021/02/11 20:50:00 ║ 1099.66  ║ 0.00154784061 ║ 709736   ║ 855001    ║ 20.227 ║ 2.41965786104 ║                     
                // ║ 2021/02/24 13:15:00 ║ 1322.09  ║ 0.00170155674 ║ 776210   ║ 748072    ║ -3.817 ║ 2.59208744460 ║                     
                // ║ 2021/02/25 17:10:00 ║ 1271.62  ║ 0.00164971863 ║ 770038   ║ 740101    ║ -4.080 ║ 2.49161765206 ║                     
                // ╚═════════════════════╩══════════╩═══════════════╩══════════╩═══════════╩════════╩═══════════════╝
                // ╔═════════════════════╦══════════╦═══════════════╦══════════╦═══════════╦════════╦═══════════════╗                     
                // ║ StartDate           ║ BuyValue ║ Quantity      ║ BuyPrice ║ SellPrice ║ Profit ║ FeeAmount     ║                     
                // ╠═════════════════════╬══════════╬═══════════════╬══════════╬═══════════╬════════╬═══════════════╣                     
                // ║ 2021/02/01 13:50:00 ║ 1000     ║ 0.00190285524 ║ 525001   ║ 592799.04 ║ 12.575 ║ 2.13000090476 ║                     
                // ║ 2021/02/09 09:30:00 ║ 1125.75  ║ 0.00158442399 ║ 709800   ║ 681408.00 ║ -4.288 ║ 2.20574989800 ║                     
                // ║ 2021/02/11 20:50:00 ║ 1077.48  ║ 0.00151662186 ║ 709736   ║ 845760.00 ║ 18.809 ║ 2.35747861104 ║                     
                // ║ 2021/02/24 13:15:00 ║ 1280.14  ║ 0.00164757078 ║ 776210   ║ 745161.60 ║ -4.288 ║ 2.51014105620 ║                     
                // ║ 2021/02/25 17:10:00 ║ 1225.25  ║ 0.00158956884 ║ 770038   ║ 739236.48 ║ -4.288 ║ 2.40525366408 ║                     
                // ╚═════════════════════╩══════════╩═══════════════╩══════════╩═══════════╩════════╩═══════════════╝
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
            Console.Out.WriteLine($"{fromDate.ToLocalTime()} to {to.ToLocalTime()} {(to-fromDate).ToShort()} ");
            backTestResult.Print();

            backTestResult.PercentProfit.Should().BeGreaterThan(expected);
            backTestResult.PercentProfit.Should().BeGreaterThan(backTestResult.PercentMarketProfit);
        }

        private async Task<StrategyInstance> BuildBackTestResult(DateTime fromDate, DateTime to, Func<IBrokerApi, IStrategy> getStrategy, string currencyPair,
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