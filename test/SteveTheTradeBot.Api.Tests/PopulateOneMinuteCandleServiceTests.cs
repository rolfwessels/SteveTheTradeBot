using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Api.Tests
{
    public class PopulateOneMinuteCandleServiceTests
    {
        private PopulateOneMinuteCandleService _service;
        private Mock<IHistoricalDataPlayer> _mockIHistoricalDataPlayer;
        private ITradePersistenceFactory _factory;

        [Test]
        public async Task Populate_GivenNoExistingRecords_ShouldLoadAllCandles()  
        {
            // arrange
            Setup();
            CancellationToken cancellationToken = default;
            var historicalTrades = Builder<HistoricalTrade>.CreateListOfSize(2).WithValidData().Build();
            var context = await _factory.GetTradePersistence();
            context.HistoricalTrades.AddRange(historicalTrades);
            context.SaveChanges();
            // action
            await _service.Populate(cancellationToken, CurrencyPair.BTCZAR, "valr");
            // assert
            _mockIHistoricalDataPlayer.VerifyAll();
            var tradeFeedCandles = _factory.GetTradePersistence().Result.TradeFeedCandles.AsQueryable().ToList();
            tradeFeedCandles.Should().HaveCount(2);
        }

        [Test]
        public async Task Populate_GivenSomeExistingRecords_ShouldSaveAllCandles()      
        {
            // arrange
            Setup();
            CancellationToken cancellationToken = default;
            var historicalTrades = Builder<HistoricalTrade>.CreateListOfSize(5).WithValidData().Build().Reverse().ToArray();
            var context = await _factory.GetTradePersistence();
            context.HistoricalTrades.AddRange(historicalTrades);
            context.SaveChanges();

            context.TradeFeedCandles.AddRange(historicalTrades.Take(2)
                .ToCandleOneMinute().Dump("")
                .Select(x => TradeFeedCandle.From(x, "valr", PeriodSize.OneMinute, CurrencyPair.BTCZAR)));
            context.SaveChanges();
            
            // action
            await _service.Populate(cancellationToken, CurrencyPair.BTCZAR, "valr");
            // assert
            _mockIHistoricalDataPlayer.VerifyAll();
            var tradeFeedCandles = context.TradeFeedCandles.AsQueryable().ToList();
            tradeFeedCandles.Should().HaveCount(5);
        }

        private void Setup()
        {
            _mockIHistoricalDataPlayer = new Mock<IHistoricalDataPlayer>();
            _factory = TestTradePersistenceFactory.UniqueDb();
            var tradeHistoryStore = new TradeHistoryStore(_factory);
            _service = new PopulateOneMinuteCandleService(_factory, new HistoricalDataPlayer(tradeHistoryStore, new TradeFeedCandlesStore(_factory)),new Messenger());
        }
    }
}