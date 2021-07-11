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
            backTestResult.TradesActive.Should().Be(1);
        }

        [Test]
        public async Task Run_Given_ShouldMakeNoTrades()
        {
            // arrange
            Setup();
            _backTestRunner = new BackTestRunner();
            var historicalDataPlayer = new HistoricalDataPlayer(new TradeHistoryStore(TestTradePersistenceFactory.RealDb()));
            var cancellationTokenSource = new CancellationTokenSource();
            
            var readHistoricalTrades = historicalDataPlayer.ReadHistoricalTrades(DateTime.Parse("2021-06-24T00:00:00.0527271+02:00"), DateTime.Parse("2021-07-10T00:00:00.0527271+02:00"), cancellationTokenSource.Token);
            // action
            var backTestResult = await _backTestRunner.Run(readHistoricalTrades, new RSiBot(), CancellationToken.None);
            // assert
            backTestResult.Dump("").TradesActive.Should().Be(1);
        }

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