using System;
using System.Linq;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Storage;

namespace SteveTheTradeBot.Core.Tests.Components.Storage
{
    [TestFixture]
    public class TradePersistenceStoreContextTests
    {

        private TradePersistenceStoreContext _tradePersistenceStoreContext;

        #region Setup/Teardown

        public void Setup()
        {
            var connection = "Host=localhost;Database=SteveTheTradeBotTestDb;Username=postgres;Password=GRES_password";
            _tradePersistenceStoreContext = new TradePersistenceStoreContext(connection);
            _tradePersistenceStoreContext.Database.EnsureCreated().Dump("sad");
        }

        #endregion

        [Test]
        public void Add_GivenHistoricalTrade_ShouldStoreRecord()
        {
            // arrange
            Setup();
            var historicalTrade = new HistoricalTrade()
            {
                Price = 259653,
                Quantity = 0.001m,
                CurrencyPair = "BTCZAR",
                TradedAt = DateTime.Parse("2020-11-30T08:51:21.604113Z"),
                TakerSide = "buy",
                SequenceId = 15822,
                Id = "1947cf61-8a47-4fce-bb75-07a7271f2f70",
                QuoteVolume = 259.653m
            };
            _tradePersistenceStoreContext.HistoricalTrades.Add(historicalTrade);
            _tradePersistenceStoreContext.SaveChangesAsync();
            // action
            var historicalTrades = _tradePersistenceStoreContext.HistoricalTrades.AsQueryable().ToList();
            // assert
            historicalTrades.Where(x => x.Id == historicalTrade.Id).Should().HaveCount(1);
        }


        
    }

}