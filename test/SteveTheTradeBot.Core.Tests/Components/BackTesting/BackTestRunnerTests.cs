﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Tests.Components.Storage;
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
            _backTestRunner = new BackTestRunner(new DynamicGraphs(TestTradePersistenceFactory.Instance));
            var list =  Builder<HistoricalTrade>.CreateListOfSize(500).WithValidData().Build();
            // action
            var backTestResult = await _backTestRunner.Run(list.ToCandleOneMinute().Aggregate(PeriodSize.OneMinute),new RSiBot(), CancellationToken.None);
            // assert
            backTestResult.TradesActive.Should().Be(0);
        }

        [Test]
        [Timeout(240000)]
        public async Task Run_Given_ShouldMakeNoTrades()
        {
            // arrange
            Setup();
            var factory = TestTradePersistenceFactory.RealDb();
            var historicalDataPlayer = new HistoricalDataPlayer(new TradeHistoryStore(factory));
            _backTestRunner = new BackTestRunner(new DynamicGraphs(factory));
            var cancellationTokenSource = new CancellationTokenSource();
            var from = DateTime.Parse("2020-01-01T00:00:00");
            var to = from.AddMonths(1);
            var readHistoricalTrades = historicalDataPlayer.ReadHistoricalData(from, to,PeriodSize.FiveMinutes, cancellationTokenSource.Token).ToList();
            // action
            var backTestResult = await _backTestRunner.Run(readHistoricalTrades, new RSiBot(), CancellationToken.None);
            // assert
            backTestResult.Dump("").BalanceMoved.Should().BeGreaterThan(backTestResult.MarketMoved);
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