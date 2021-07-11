using System;
using System.Linq;
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
            var list =  Builder<HistoricalTrade>.CreateListOfSize(500).WithValidData().Build();
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
            
            var historicalDataPlayer = new HistoricalDataPlayer(new TradeHistoryStore(TestTradePersistenceFactory.RealDb()));
            var cancellationTokenSource = new CancellationTokenSource();
            var from = DateTime.Parse("2021-06-01T00:00:00");
            var to = from.AddDays(30);
            var readHistoricalTrades = historicalDataPlayer.ReadHistoricalTrades(from, to, cancellationTokenSource.Token).ToList();
            // action
            var backTestResult = await _backTestRunner.Run(readHistoricalTrades, new RSiBot(), CancellationToken.None);
            // assert
            backTestResult.Dump("").TradesActive.Should().Be(1);
            backTestResult.Balance.Should().Be(1090.33m);
        }

        // [15:27:39 INF] 2021/06/25 12:51:00 Send signal to buy at 476572 Rsi:19.89768218513764655784909133
        // [15:27:39 INF] 2021/06/26 09:40:00 Send signal to sell at 458148 Rsi:80.62350397702059448599768854
        // [15:27:40 INF] 2021/06/26 14:02:00 Send signal to buy at 447654 Rsi:18.83686390847585654995976824
        // [15:27:40 INF] 2021/06/27 22:05:00 Send signal to sell at 484910 Rsi:82.16131918634721797678475347
        // [15:27:40 INF] 2021/06/28 17:40:00 Send signal to buy at 498363 Rsi:14.66734205797910560010460738
        // [15:27:40 INF] 2021/06/28 20:35:00 Send signal to sell at 508079 Rsi:80.16137413360774618162572650
        #region Setup/Teardown

        public void Setup()
        {
            TestLoggingHelper.EnsureExists();
            _backTestRunner = new BackTestRunner();
        }

        [TearDown]
        public void TearDown()
        {

        }

        #endregion
    }

    
}