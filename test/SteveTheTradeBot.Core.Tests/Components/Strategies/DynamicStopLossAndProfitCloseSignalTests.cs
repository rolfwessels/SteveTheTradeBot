using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Tests.Components.BackTesting;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Components.Strategies
{
    public class DynamicStopLossAndProfitCloseSignalTests
    {
        private DynamicStopLossAndProfitCloseSignal _sut;
        private StrategyContext _strategyContext;
        private FakeBroker _fakeBroker;

        [Test]
        public async Task Initialize_GivenLowerThenHighThenLow_ShouldPickLastLow()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] { 1000, 1100, 1230, 1200, 1100 }));
            // action
            await _sut.Initialize(_strategyContext, 1120, fakeStrategy);
            // assert
            var stopOrders = _fakeBroker.Requests.Dump("").OfType<StopLimitOrderRequest>().ToArray();
            stopOrders.Should().HaveCount(1);
            stopOrders.First().Price.Should().Be(1100);
        }


        [Test]
        public async Task Initialize_GivenValue_ShouldSetRiskValue()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] { 1000, 1100, 1230, 1200, 1100 }));
            // action
            await _sut.Initialize(_strategyContext, 1120, fakeStrategy);
            // assert
            var stopOrders = await _strategyContext.Get("Risk", 0m);
            stopOrders.Should().Be(0.01818m);
        }

        [Test]
        public async Task Initialize_GivenHighThenLowerThenLow_ShouldPickLastLowest()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] {  1100, 1230, 1000, 1200, 1100 }));
            // action
            await _sut.Initialize(_strategyContext, 1010, fakeStrategy);
            // assert
            var stopOrders = _fakeBroker.Requests.Dump("").OfType<StopLimitOrderRequest>().ToArray();
            stopOrders.Should().HaveCount(1);
            stopOrders.First().Price.Should().Be(1000);
        }

        [Test]
        public async Task Initialize_GivenLowThatIsMoreThanMaxLowPercent_ShouldMaxLowAmount()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] { 1100, 1230, 1000, 1200, 1100 }));
            // action
            await _sut.Initialize(_strategyContext, 1150, fakeStrategy);
            // assert
            var stopOrders = _fakeBroker.Requests.Dump("").OfType<StopLimitOrderRequest>().ToArray();
            stopOrders.First().Price.Should().Be((1-_sut.MaxRisk) * 1150m);
        }

        [Test]
        public async Task Initialize_GivenQuotesHigherThan_ShouldMaxLowAmount()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] { 1170, 1170 }));
            // action
            await _sut.Initialize(_strategyContext, 1150, fakeStrategy);
            // assert
            var stopOrders = _fakeBroker.Requests.Dump("").OfType<StopLimitOrderRequest>().ToArray();
            stopOrders.First().Price.Should().Be(1138M);
        }

        [Test]
        public async Task DetectClose_GivenQuoteLowerThanStopLoss_ShouldDoNothing()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] {1150}));
            await _sut.Initialize(_strategyContext, 1150, fakeStrategy);
            // action
            await _sut.DetectClose(
                _strategyContext, 
                BuildOrders(new[] {1150}).Last(), 
                _strategyContext.ActiveTrade(),
                fakeStrategy);
            // assert
            _fakeBroker.Requests.Should().HaveCount(1);
        }

        [Test]
        public async Task DetectClose_Given2TimesTheRisk_ShouldRaiseTheStopLoss()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] { 1150 }));
            await _sut.Initialize(_strategyContext, 1000, fakeStrategy);
            var risk = await _strategyContext.Get("Risk", 0);
            risk.Should().Be(0.0101m);
            var nextStopLoss = await _strategyContext.Get("NextStopLoss", 0);
            nextStopLoss.Should().Be(1010.10m);
            var currentTrade = BuildOrders(new[] { 1011 }).Last();
            // action
            await _sut.DetectClose(
                _strategyContext,
                currentTrade,
                _strategyContext.ActiveTrade(),
                fakeStrategy);
            // assert
            var stopOrders = _fakeBroker.Requests.Skip(1).OfType<StopLimitOrderRequest>().ToArray();
            stopOrders.First().Price.Should().Be(1000M);
        }

        [Test]
        public async Task DetectClose_GivenPriceAboveExit_ShouldSell()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] { 1150 }));
            await _sut.Initialize(_strategyContext, 1000, fakeStrategy);
            var exitAt = await _strategyContext.Get("ExitAt", 0);
            exitAt.Should().Be(1030.30m);
            var currentTrade = BuildOrders(new[] { (1031) }).Last();
            // action
            await _sut.DetectClose(
                _strategyContext,
                currentTrade,
                _strategyContext.ActiveTrade(),
                fakeStrategy);
            // assert
            var simpleOrderRequests = _fakeBroker.Requests.OfType<SimpleOrderRequest>().ToArray();
            simpleOrderRequests.Should().HaveCount(1);
            simpleOrderRequests.First().Side.Should().Be(Side.Sell);
        }

        #region Private Methods

        private IEnumerable<TradeQuote> BuildOrders(int[] values)
        {
            return values.Select((value, index) => new TradeQuote
            {
                Feed = "valr",
                PeriodSize = PeriodSize.FiveMinutes,
                Date = DateTime.Now.AddDays(-values.Length + index),
                Open = value,
                High = value,
                Low = value,
                Close = value,
                Volume = index,
                CurrencyPair = CurrencyPair.ETHZAR
            });
        }

        private void Setup()
        {
            var forBackTest = StrategyInstance.ForBackTest("abc",CurrencyPair.XRPZAR,100);
            var dynamicGraphs = new DynamicGraphsTests.FakeGraph();
            _fakeBroker = new FakeBroker(new Messenger());
            _strategyContext = new StrategyContext(dynamicGraphs, forBackTest,_fakeBroker, new Messenger(),new ParameterStore(TestTradePersistenceFactory.InMemoryDb));
            _sut = new DynamicStopLossAndProfitCloseSignal();
        }

        #endregion
    }
}