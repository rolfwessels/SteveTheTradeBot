using System;
using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Components.Strategies
{
    public class MacdCloseSignalTests : SignalCloseTestBase
    {
        private MacdCloseSignal _sut;

        [Test]
        public async Task Initialize_GivenLowerThenHighThenLow_ShouldPickLastLow()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] { 1000m, 1100, 1230, 1200, 1100 }));
            // action
            await _sut.Initialize(_strategyContext, 1120, fakeStrategy);
            // assert
            var stopOrders = _fakeBroker.Requests.Dump("").OfType<StopLimitOrderRequest>().ToArray();
            stopOrders.Should().HaveCount(1);
            stopOrders.First().Price.Should().Be(1100);
        }

        [Test]
        public async Task DetectClose_GivenPriceBelowNextStopLoss_ShouldDoNothing()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] { 1150m }));
            await _sut.Initialize(_strategyContext, 1000, fakeStrategy);
            var stopLoss = await _strategyContext.Get(StrategyProperty.StopLoss,0m);
            var currentTrade = BuildOrders(new[] { stopLoss }).Last();
            // action
            await _sut.DetectClose(
                _strategyContext,
                currentTrade,
                _strategyContext.ActiveTrade(),
                fakeStrategy);
            // assert
            _fakeBroker.Requests.Should().HaveCount(1);
        }


        [Test]
        public async Task DetectClose_GivenFirstPriceUpdateStopLossAt_ShouldSetStopLossToBreakEven()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] { 1150m }));
            await _sut.Initialize(_strategyContext, 1000, fakeStrategy);
            var updateStopLossAt = await _strategyContext.Get(StrategyProperty.UpdateStopLossAt, 0m);
            
            // action
            await DetectCloseAt(updateStopLossAt+1, fakeStrategy);
            // assert
            var simpleOrderRequests = _fakeBroker.Requests.OfType<StopLimitOrderRequest>().ToArray();
            simpleOrderRequests.Should().HaveCount(2);
            simpleOrderRequests.Last().Price.Should().Be(_strategyContext.ActiveTrade().BuyPrice * 1.001m);
        }


        [Test]
        public async Task DetectClose_GivenFirstStopLossReached_ShouldNotResetStopLoss()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] { 1150m }));
            await _sut.Initialize(_strategyContext, 1000, fakeStrategy);
            var firstUpdateStopLoss = await _strategyContext.Get(StrategyProperty.UpdateStopLossAt, 0m);
            await DetectCloseAt(firstUpdateStopLoss + 1, fakeStrategy);
            // action
            await DetectCloseAt(firstUpdateStopLoss + 10, fakeStrategy);
            // assert
            _fakeBroker.Requests.OfType<StopLimitOrderRequest>().Should().HaveCount(2);
            _fakeBroker.Requests.OfType<SimpleOrderRequest>().Should().HaveCount(0);
        }
        
        [Test]
        public async Task DetectClose_GivenFirstStopLossReached_ShouldLookForMacdReverse()
        {
            // arrange
            Setup();
            var fakeStrategy = new FakeStrategy();
            _strategyContext.StrategyInstance.AddTrade(DateTime.Now, 1000, 1);
            _strategyContext.ByMinute.AddRange(BuildOrders(new[] { 1150m }));
            await _sut.Initialize(_strategyContext, 1000, fakeStrategy);
            var firstUpdateStopLoss = await _strategyContext.Get(StrategyProperty.UpdateStopLossAt, 0m);
            await DetectCloseAt(firstUpdateStopLoss + 1, fakeStrategy);
            var tradeQuotes = Builder<TradeQuote>.CreateListOfSize(2)
                .All()
                
                .With((x, i) => x.Metric.Add(Signals.MacdValue, 0 - i))
                .With((x, i) => x.Metric.Add(Signals.MacdSignal, 0))
                .Build().Dump("d");
            _strategyContext.ByMinute.AddRange(tradeQuotes);
            // action
            await _sut.DetectClose(_strategyContext, _strategyContext.ByMinute.Last(), _strategyContext.ActiveTrade(), fakeStrategy);
            // assert
            var simpleOrderRequests = _fakeBroker.Requests.OfType<SimpleOrderRequest>().ToArray();
            simpleOrderRequests.Should().HaveCount(1);
            simpleOrderRequests.First().Side.Should().Be(Side.Sell);
        }

        private async Task DetectCloseAt(decimal updateStopLossAt, FakeStrategy fakeStrategy)
        {
            var currentTrade = BuildOrders(new[] {updateStopLossAt}).Last();
            await _sut.DetectClose(_strategyContext, currentTrade, _strategyContext.ActiveTrade(), fakeStrategy);
        }


        protected override void Setup()
        {
            base.Setup();
            _sut = new MacdCloseSignal();
        }
    }
}