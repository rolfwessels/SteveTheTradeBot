using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Components.Strategies
{
    public class RaiseStopLossCloseSignalTests : SignalCloseTestBase
    {
        private RaiseStopLossCloseSignal _sut;
        private readonly RSiStrategy _rSiStrategy = new RSiStrategy();

        #region Setup/Teardown

        protected override void Setup()
        {
            base.Setup();
            _sut = new RaiseStopLossCloseSignal(0.99m, 1.02m);
        }

        #endregion

        [Test]
        public async Task Initialize_GivenGivenContext_ShouldSetStopLoss()
        {
            // arrange
            Setup();
            // action
            await _sut.Initialize(_strategyContext, 1000, _rSiStrategy);
            // assert
            var stopLoss = await _strategyContext.Get(StrategyProperty.StopLoss, 0m);
            stopLoss.Should().Be(990m);
        }


        [Test]
        public async Task Initialize_GivenGivenContext_ShouldSetStopLossRecord()
        {
            // arrange
            Setup();
            var boughtAtPrice = 1000;
            var (addTrade, tradeOrder) = _strategyContext.StrategyInstance.AddBuyTradeOrder(100, boughtAtPrice, DateTime.Now);
            _strategyContext.ByMinute.AddRange(BuildOrders(995));
            // action
            await _sut.Initialize(_strategyContext, 1000, _rSiStrategy);
            // assert
            var stopLoss = _strategyContext.StrategyInstance.ActiveTrade().GetValidStopLoss();
            _strategyContext.StrategyInstance.Print();
            stopLoss.OrderPrice.Should().Be(990m);
            stopLoss.StopPrice.Should().BeApproximately(991m,0.1m);
        }

        [Test]
        public async Task Initialize_GivenGivenContext_ShouldSetStopLossRecordOnApi()
        {
            // arrange
            Setup();
            var boughtAtPrice = 1000;
            var (addTrade, tradeOrder) = _strategyContext.StrategyInstance.AddBuyTradeOrder(100, boughtAtPrice, DateTime.Now);
            _strategyContext.ByMinute.AddRange(BuildOrders(995));
            // action
            await _sut.Initialize(_strategyContext, 1000, _rSiStrategy);
            // assert
            var stopLoss = _fakeBroker.Requests.OfType<StopLimitOrderRequest>().First();
            _strategyContext.StrategyInstance.Print();
            stopLoss.Price.Should().Be(990m);
            stopLoss.StopPrice.Should().Be(991m);
        }


        [Test]
        public async Task Initialize_GivenGivenContext_ShouldSetUpdateStopLossAt()
        {
            // arrange
            Setup();
            // action
            await _sut.Initialize(_strategyContext, 1000, _rSiStrategy);
            // assert
            var updateAt = await _strategyContext.Get(StrategyProperty.UpdateStopLossAt, 0m);
            updateAt.Should().Be(1020.00m);
        }

        [Test]
        public async Task DetectClose_GivenCloseUnderAll_ShouldDoNothing()
        {
            // arrange
            Setup();
            var boughtAtPrice = 1000;
            var (addTrade, tradeOrder) = _strategyContext.StrategyInstance.AddBuyTradeOrder(100, boughtAtPrice, DateTime.Now);
            _strategyContext.ByMinute.AddRange(BuildOrders(995));
            await _sut.Initialize(_strategyContext, boughtAtPrice, _rSiStrategy);
            // action
            await _sut.DetectClose(_strategyContext, _strategyContext.LatestQuote(), addTrade, _rSiStrategy);
            // assert
            _strategyContext.StrategyInstance.Status.Should().Be("Waiting for price above 1020.00 or stop loss 990.00");
        }

        [Test]
        public async Task DetectClose_GivenOverUpdateLossAt_ShouldUpdateTheStopLoss()
        {
            // arrange
            Setup();
            var boughtAtPrice = 1000;
            var (addTrade, tradeOrder) = _strategyContext.StrategyInstance.AddBuyTradeOrder(100, boughtAtPrice, DateTime.Now);
            _strategyContext.ByMinute.AddRange(BuildOrders(1020.00m));
            await _sut.Initialize(_strategyContext, boughtAtPrice, _rSiStrategy);
            // action
            await _sut.DetectClose(_strategyContext, _strategyContext.LatestQuote(), addTrade, _rSiStrategy);
            // assert
            var stopLoss = await _strategyContext.Get(StrategyProperty.StopLoss, 0m);
            stopLoss.Should().Be(1009.8m);
        }


        [Test]
        public async Task DetectClose_GivenOverUpdateLossAt_ShouldUpdateStopLossAt()
        {
            // arrange
            Setup();
            var boughtAtPrice = 1000;
            var (addTrade, tradeOrder) = _strategyContext.StrategyInstance.AddBuyTradeOrder(100, boughtAtPrice, DateTime.Now);
            _strategyContext.ByMinute.AddRange(BuildOrders(1020.00m));
            await _sut.Initialize(_strategyContext, boughtAtPrice, _rSiStrategy);
            // action
            await _sut.DetectClose(_strategyContext, _strategyContext.LatestQuote(), addTrade, _rSiStrategy);
            // assert
            var updateStopLossAt = await _strategyContext.Get(StrategyProperty.UpdateStopLossAt, 0m);
            updateStopLossAt.Should().Be(1040.400m);
        }

        [Test]
        public async Task DetectClose_GivenOverUpdateLossAt_ShouldSetStatus()
        {
            // arrange
            Setup();
            var boughtAtPrice = 1000;
            var (addTrade, tradeOrder) = _strategyContext.StrategyInstance.AddBuyTradeOrder(100, boughtAtPrice, DateTime.Now);
            _strategyContext.ByMinute.AddRange(BuildOrders(1020.00m));
            await _sut.Initialize(_strategyContext, boughtAtPrice, _rSiStrategy);
            // action
            await _sut.DetectClose(_strategyContext, _strategyContext.LatestQuote(), addTrade, _rSiStrategy);
            // assert
            _strategyContext.StrategyInstance.Status.Should().Be("Update stop loss to 1009.8000 that means guaranteed profit of 0.980%");
        }

        
    }
}