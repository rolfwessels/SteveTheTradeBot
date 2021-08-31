using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core.Tests.Components.Strategies
{
    public class RaiseStopLossCloseSignalDynamicTests : SignalCloseTestBase
    {
        private RaiseStopLossCloseSignalDynamic _sut;
        private readonly RSiStrategy _rSiStrategy = new RSiStrategy();

        #region Setup/Teardown

        protected override void Setup()
        {
            base.Setup();
            _sut = new RaiseStopLossCloseSignalDynamic(0.01m, 0.01m, 0.05m);
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
        public async Task Initialize_GivenGivenContext_ShouldSetUpdateStopLossAt()
        {
            // arrange
            Setup();
            // action
            await _sut.Initialize(_strategyContext, 1000, _rSiStrategy);
            // assert
            var updateAt = await _strategyContext.Get(StrategyProperty.UpdateStopLossAt, 0m);
            updateAt.Should().Be(1010.00m);
        }

        [Test]
        public async Task Initialize_GivenGivenContext_ShouldSetRiskStatus()
        {
            // arrange
            Setup();
            // action
            await _sut.Initialize(_strategyContext, 1000, _rSiStrategy);
            // assert
            _strategyContext.StrategyInstance.Status.Should().Be("Set stop loss to 990.00 that means risk of 1.00%");
        }

        [Test]
        public async Task DetectClose_GivenCloseUnderAll_ShouldDoNothing()
        {
            // arrange
            Setup();
            var boughtAtPrice = 1000;
            var (addTrade, tradeOrder) = _strategyContext.StrategyInstance.AddBuyTradeOrder(100, boughtAtPrice, DateTime.Now);
            _strategyContext.Quotes.AddRange(BuildOrders(995));
            await _sut.Initialize(_strategyContext, boughtAtPrice, _rSiStrategy);
            // action
            await _sut.DetectClose(_strategyContext, _strategyContext.LatestQuote(), addTrade, _rSiStrategy);
            // assert
            _strategyContext.StrategyInstance.Status.Should().Be("Waiting for price above 1010.00 or stop loss 990.00");
        }

        [Test]
        public async Task DetectClose_GivenOverUpdateLossAt_ShouldUpdateTheStopLoss()
        {
            // arrange
            Setup();
            var boughtAtPrice = 1000;
            var (addTrade, tradeOrder) = _strategyContext.StrategyInstance.AddBuyTradeOrder(100, boughtAtPrice, DateTime.Now);
            _strategyContext.Quotes.AddRange(BuildOrders(1020.00m));
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
            _strategyContext.Quotes.AddRange(BuildOrders(1020.00m));
            await _sut.Initialize(_strategyContext, boughtAtPrice, _rSiStrategy);
            // action
            await _sut.DetectClose(_strategyContext, _strategyContext.LatestQuote(), addTrade, _rSiStrategy);
            // assert
            var updateStopLossAt = await _strategyContext.Get(StrategyProperty.UpdateStopLossAt, 0m);
            updateStopLossAt.Should().Be(1030.20m);
        }

        [Test]
        public async Task DetectClose_GivenOverUpdateLossAt_ShouldSetStatus()
        {
            // arrange
            Setup();
            var boughtAtPrice = 1000;
            var (addTrade, tradeOrder) = _strategyContext.StrategyInstance.AddBuyTradeOrder(100, boughtAtPrice, DateTime.Now);
            _strategyContext.Quotes.AddRange(BuildOrders(1020.00m));
            await _sut.Initialize(_strategyContext, boughtAtPrice, _rSiStrategy);
            // action
            await _sut.DetectClose(_strategyContext, _strategyContext.LatestQuote(), addTrade, _rSiStrategy);
            // assert
            _strategyContext.StrategyInstance.Status.Should().Be("Update stop loss to 1009.8000 that means guaranteed profit of 0.980%");
        }

        [Test]
        public async Task DetectClose_GivenLinearGrowth_ShouldSetStopLossAtIntervals()
        {
            // arrange
            Setup();
            var boughtAtPrice = 1000;
            var (addTrade, tradeOrder) = _strategyContext.StrategyInstance.AddBuyTradeOrder(100, boughtAtPrice, DateTime.Now);
            _strategyContext.Quotes.AddRange(BuildOrders(1000));
            await _sut.Initialize(_strategyContext, boughtAtPrice, _rSiStrategy);
            var result = new Dictionary<decimal, decimal>();
            var expected = new Dictionary<decimal, decimal> {
                {1000m, 990.00m},
                {1010m, 999.90m},
                {1015m, 999.90m},
                {1020m, 999.90m},
                {1050.00m, 1016.82m},
                {1065.00m, 1018.69380000M},
                {1087m, 1032.65M},
                {1110m, 1054.50m},
                {1210m, 1149.50m},
                {1510m, 1434.50m},
                {2000m, 1900.00m},
            };
            // action
            foreach (var values in expected)
            {
                _strategyContext.Quotes.AddRange(BuildOrders(values.Key));
                await _sut.DetectClose(_strategyContext, _strategyContext.LatestQuote(), addTrade, _rSiStrategy);
                var stopLoss = await _strategyContext.Get(StrategyProperty.StopLoss, 0m);
                var updateAt = await _strategyContext.Get(StrategyProperty.UpdateStopLossAt, 0m);
                result.Add(values.Key, stopLoss);

            }

            // assert
            Console.Out.WriteLine(result.Select(x => new {
                    CurrentPrice = x.Key,
                    StopLoss = x.Value,
                    Profit = TradeUtils.MovementPercent(x.Key, boughtAtPrice),
                    Risk = TradeUtils.MovementPercent(x.Value, boughtAtPrice),
                    Diff = TradeUtils.MovementPercent(x.Value, x.Key) 
                }

            ).ToTable());
            result.Should().BeEquivalentTo(expected);
            

        }
    }

    
}