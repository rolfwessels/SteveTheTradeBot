using System;
using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.Broker
{
    public class UpdateHistoricalDataTests
    {
        
        private UpdateHistoricalData _updateHistoricalData;
        private Mock<IHistoricalDataApi> _mockIHistoricalDataApi;
        private TradePersistenceStoreContext _tradePersistenceStoreContext;

        [Test] 
        public async Task StartUpdate_GivenACall_ShouldRepeatedlyCallToGetHistoricalData()
        {
            // arrange
            await Setup();
            var currencyPair = $"{CurrencyPair.BTCZAR}-{Guid.NewGuid()}";
            var allItems = Builder<HistoricalTrade>.CreateListOfSize(400).WithValidData().Build().ForEach(x=>x.CurrencyPair = currencyPair);
            _updateHistoricalData.BatchSize = 50;
            
            _mockIHistoricalDataApi.Setup(mc => mc.GetTradeHistory(currencyPair, 0, _updateHistoricalData.BatchSize))
                .Returns(() => Task.FromResult(allItems.Select(x=>MapCore.ToDto(x)).Take(_updateHistoricalData.BatchSize).ToArray()));
            var skip = 0;
            _mockIHistoricalDataApi.Setup(mc => mc.GetTradeHistory(currencyPair, It.IsAny<string>(), _updateHistoricalData.BatchSize))
                .Returns(() =>
                {
                    skip += _updateHistoricalData.BatchSize;
                    return Task.FromResult(allItems.Select(x => x.ToDto()).Skip(skip)
                            .Take(_updateHistoricalData.BatchSize).ToArray());
                });

            // action
            await _updateHistoricalData.PopulateNewThenOld(currencyPair, default);
            // assert
            var historicalTrades = _tradePersistenceStoreContext.HistoricalTrades.AsQueryable().Where(x=>x.CurrencyPair == currencyPair).ToList();
            historicalTrades.Count.Should().Be(400);
        }


        [Test]
        public async Task StartUpdate_GivenASomeExisting_ShouldAddNewDataThenTryLoadHistoricalData()
        {
            // arrange
            await Setup();
            var currencyPair = $"{CurrencyPair.BTCZAR}-{Guid.NewGuid()}";
            var allItems = Builder<HistoricalTrade>.CreateListOfSize(400).WithValidData().Build().ForEach(x => x.CurrencyPair = currencyPair);
            _updateHistoricalData.BatchSize = 50;
            var dbContext = await TestTradePersistenceFactory.InMemoryDb.GetTradePersistence();
            dbContext.HistoricalTrades.AddRange(allItems.Skip(110).Take(30));
            dbContext.SaveChanges();

            _mockIHistoricalDataApi.Setup(mc => mc.GetTradeHistory(currencyPair, 0, _updateHistoricalData.BatchSize))
                .Returns(() => Task.FromResult(allItems.Select(x => x.ToDto()).Take(_updateHistoricalData.BatchSize).ToArray()));
            var skip = 0;
            _mockIHistoricalDataApi.Setup(mc => mc.GetTradeHistory(currencyPair, It.IsAny<string>(), _updateHistoricalData.BatchSize))
                .Returns(() =>
                {
                    skip += _updateHistoricalData.BatchSize;
                    return Task.FromResult(allItems.Select(x => x.ToDto()).Skip(skip)
                        .Take(_updateHistoricalData.BatchSize).ToArray());
                });

            // action
            await _updateHistoricalData.PopulateNewThenOld(currencyPair, default);
            // assert
            var historicalTrades = _tradePersistenceStoreContext.HistoricalTrades.AsQueryable().Where(x => x.CurrencyPair == currencyPair).ToList();
            historicalTrades.Count.Should().Be(400);
        }

        private async Task Setup()
        {
            TestLoggingHelper.EnsureExists();
            _mockIHistoricalDataApi = new Mock<IHistoricalDataApi>();
            _tradePersistenceStoreContext = await TestTradePersistenceFactory.InMemoryDb.GetTradePersistence();
            _updateHistoricalData = new UpdateHistoricalData(_mockIHistoricalDataApi.Object, new TradeHistoryStore(TestTradePersistenceFactory.InMemoryDb));
        }
    }
}