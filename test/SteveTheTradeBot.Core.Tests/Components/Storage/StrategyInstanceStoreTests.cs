using System;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;
using FluentAssertions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

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


        [Test]
        public async Task Update_GivenTwoContexts_ShouldUpdateTheInstance() 
        {
            // arrange
            Setup();
            var strategyInstances = Builder<StrategyInstance>.CreateNew()
                .WithValidData()
                .With(x => x.Reference = "Reg" + GetRandom.String(30))
                .Build();
            _store = new StrategyInstanceStore(TestTradePersistenceFactory.PostgresTest);
            await _store.Add(strategyInstances);
            // action
            var result = await _store.FindById(strategyInstances.Id);
            await _store.EnsureUpdate(strategyInstances.Id, s => { 
                s.AddTrade(DateTime.Now, 123, 32, 1);
                return Task.FromResult(true);
            });
            await _store.EnsureUpdate(strategyInstances.Id, s =>
            {
                s.Trades.Should().HaveCount(1);
                s.AddTrade(DateTime.Now, 123, 32, 1);
                return Task.FromResult(true);
            });
            var result2 = await _store.FindById(strategyInstances.Id);
            // assert
            result.Should().HaveCount(1);
            result2[0].Trades.Should().HaveCount(2);
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