using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Components.Storage
{
    public class StrategyInstanceStoreTests
    {
        private StrategyInstanceStore _store;

        [Test]
        public async Task FindActiveByPairAndPeriod_GivenPairAndCurrency_ShouldReturnOnlyActive()
        {
            // arrange
            Setup();
            var strategyInstances = Builder<StrategyInstance>.CreateListOfSize(2)
                .All()
                .With(x => x.PeriodSize = PeriodSize.FiveMinutes)
                .With(x=>x.Pair = CurrencyPair.BTCZAR)
                .With(x => x.IsBackTest = false)
                .TheLast(1).With(x=>x.IsActive = true)
                .Build();
            await _store.AddRange(strategyInstances);
            // action
            var result = await _store.FindActiveStrategies();
            // assert
            result.Should().HaveCount(1);
        }

        [Test]
        public async Task FindActiveByPairAndPeriod_GivenPairAndCurrency_ShouldNotReturnBackTests()
        {
            // arrange
            Setup();
            var strategyInstances = Builder<StrategyInstance>.CreateListOfSize(3)
                .All()
                .With(x => x.PeriodSize = PeriodSize.FiveMinutes)
                .With(x=>x.Pair = CurrencyPair.BTCZAR)
                .With(x => x.IsActive = true)
                .TheLast(2).With(x=>x.IsBackTest = true)
                .Build();
            await _store.AddRange(strategyInstances);
            // action
            var result = await _store.FindActiveStrategies();
            // assert
            result.Should().HaveCount(1);
        }

        #region Setup/Teardown

        public void Setup()
        {
            _store = new StrategyInstanceStore(TestTradePersistenceFactory.UniqueDb());
        }

        [TearDown]
        public void TearDown()
        {

        }

        #endregion


    }
}