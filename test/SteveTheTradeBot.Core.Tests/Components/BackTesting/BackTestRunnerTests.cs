using System;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.BackTesting
{

    public class BackTestRunnerTests
    {
        private BackTestRunner _backTestRunner;

        [Test]
        public async Task Run_GivenSmallAmountOfData_ShouldMakeNoTrades()
        {
            // arrange
            Setup();
            _backTestRunner = new BackTestRunner();
            var list =  Builder<HistoricalTrade>.CreateListOfSize(500).WithValidData().Build().ToAsyncEnumerable();
            // action
            var backTestResult = await _backTestRunner.Run(list, new RSiBot(), CancellationToken.None);
            // assert
            backTestResult.Should().Be(1);
        }

        [Test]
        public async Task Run_Given_ShouldMakeNoTrades()
        {
            // arrange
            Setup();
            _backTestRunner = new BackTestRunner();
            var historicalDataPlayer = new HistoricalDataPlayer(new TradeHistoryStore(TestTradePersistenceFactory.RealDb()));
            var cancellationTokenSource = new CancellationTokenSource();
            var from = DateTime.Parse("2021-06-24T00:00:00.0527271+02:00");
            var to = from.AddDays(9);
            var readHistoricalTrades = historicalDataPlayer.ReadHistoricalTrades(from, to, cancellationTokenSource.Token);
            // action
            var backTestResult = await _backTestRunner.Run(readHistoricalTrades, new RSiBot(), CancellationToken.None);
            // assert
            backTestResult.TradesActive.Should().Be(0);
            backTestResult.Balance.Should().Be(1034.48M);
        }

        // [14:08:46 INF] ReadHistoricalTrades 2021/06/24 00:00:00 2021/07/10 00:00:00
        // [14:16:32 INF] 2021/06/25 16:28:00 Send signal to buy at 465101 Rsi:17.00808352999054167787767424
        // [14:16:33 INF] 2021/06/26 09:40:00 Send signal to sell at 458148 Rsi:80.64688798128113082521113648
        // [14:16:33 INF] 2021/06/26 14:02:00 Send signal to buy at 447654 Rsi:18.84410957292915814324564258
        // [14:16:33 INF] 2021/06/27 11:33:00 Send signal to sell at 481993 Rsi:80.36992416435709542882353348
        // [14:16:34 INF] 2021/06/27 18:10:00 Send signal to buy at 473636 Rsi:19.92015325315979870200135274
        // [14:16:34 INF] 2021/06/27 22:05:00 Send signal to sell at 484910 Rsi:82.16215529172851788114820654
        // [14:16:34 INF] 2021/06/28 17:40:00 Send signal to buy at 498363 Rsi:14.74626413772815935971646533
        // [14:16:34 INF] 2021/06/28 20:35:00 Send signal to sell at 508080 Rsi:80.08874897785560681223962841
        // [14:16:35 INF] 2021/06/29 22:28:00 Send signal to buy at 523099 Rsi:19.36340912905971842504100316
        // [14:16:36 INF] 2021/06/30 20:05:00 Send signal to sell at 509013 Rsi:81.42459535831544627075200586
        // [14:16:36 INF] 2021/06/30 20:52:00 Send signal to buy at 503180 Rsi:18.80564170092589671755786068
        // [14:16:36 INF] 2021/07/02 07:45:00 Send signal to sell at 495999 Rsi:82.49847027394931359096785133
        #region Setup/Teardown

        public void Setup()
        {
            TestLoggingHelper.EnsureExists();
        }

        [TearDown]
        public void TearDown()
        {

        }

        #endregion
    }

    
}