using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.Storage
{
    public class TradeHistoryStoreTests
    {
        private TradeHistoryStore _tradeHistoryStore;

        #region Setup/Teardown

        public void Setup()
        {
            _tradeHistoryStore = new TradeHistoryStore(TestTradePersistenceFactory.UniqueDb());
        }

        #endregion

        [Test]
        public async Task GetExistingRecords_Given4Records_ShouldGetEarliestAndLatest()
        {
            // arrange
            Setup();
            var historicalTrades = Builder<HistoricalTrade>.CreateListOfSize(4).WithValidData().Build();
            await _tradeHistoryStore.AddRangeAndIgnoreDuplicates(historicalTrades.ToList());
            // action
            var existingRecords = await _tradeHistoryStore.GetExistingRecords(CurrencyPair.BTCZAR);
            // assert
            existingRecords.earliest.TradedAt.Should().BeBefore(existingRecords.latest.TradedAt);
            existingRecords.earliest.Id.Should().Be(historicalTrades.Last().Id);
            existingRecords.latest.Id.Should().Be(historicalTrades.First().Id);
        }


        [Test]
        public async Task AddRangeAndIgnoreDuplicates_GivenPartial_ShouldInsertTheRest()
        {
            // arrange
            Setup();
            var historicalTrades = Builder<HistoricalTrade>.CreateListOfSize(4).WithValidData().Build();
            await _tradeHistoryStore.AddRangeAndIgnoreDuplicates(historicalTrades.Take(2).ToList());
            // action
            var existingRecords = await _tradeHistoryStore.AddRangeAndIgnoreDuplicates(historicalTrades.ToList());
            // assert
            var findById = await _tradeHistoryStore.FindById(historicalTrades.Select(x => x.Id));
            findById.Should().HaveCount(4);
        }


    }
}