using System;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.Storage
{
    [TestFixture]
    public class TradePersistenceStoreContextTests
    {

        private TradePersistenceStoreContext _tradePersistenceStoreContext;

        #region Setup/Teardown

        public void Setup()
        {
            _tradePersistenceStoreContext = TestTradePersistenceFactory.InMemoryDb.GetTestDb().Result;
        }

        #endregion

        [Test]
        public async Task Add_GivenHistoricalTrade_ShouldStoreRecord()
        {
            // arrange
            Setup();
            var historicalTrade = Builder<HistoricalTrade>.CreateNew().WithValidData().Build();
            _tradePersistenceStoreContext.HistoricalTrades.Add(historicalTrade);
            await _tradePersistenceStoreContext.SaveChangesAsync();
            // action
            var historicalTrades = _tradePersistenceStoreContext.HistoricalTrades.AsQueryable().ToList();
            // assert
            historicalTrades.Where(x => x.Id == historicalTrade.Id).Should().HaveCount(1);
        }
        
        
        [Test]
        public async Task Add_GivenTradeFeedCandles_ShouldStoreRecord()
        {
            // arrange
            Setup();
            var historicalTrade = Builder<HistoricalTrade>.CreateListOfSize(10).WithValidData().Build().ToCandleOneMinute();
            var feedName = "test"+Guid.NewGuid().ToString("n");
            _tradePersistenceStoreContext.TradeFeedCandles.AddRange(historicalTrade.Select(x=>TradeFeedCandle.From(x,feedName,Skender.Stock.Indicators.PeriodSize.OneMinute, "BTCZAR")));
            await _tradePersistenceStoreContext.SaveChangesAsync();
            // action
            var historicalTrades = _tradePersistenceStoreContext.TradeFeedCandles.AsQueryable().ToList();
            // assert
            historicalTrades.Where(x => x.Feed == feedName).Should().HaveCount(10);
        }


        
    }

}