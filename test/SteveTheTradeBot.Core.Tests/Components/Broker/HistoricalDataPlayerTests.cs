using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.Broker
{
    public class HistoricalDataPlayerTests
    {
        [Test]
        public async Task ReadHistoricalTrades_GivenTwoDate_ShouldReturnAllResult()
        {
            // arrange
            TestLoggingHelper.EnsureExists();
            var tradeHistoryStore = new TradeHistoryStore(TestTradePersistenceFactory.InMemoryDb);
            var expected = 10;
            var historicalTrades = Builder<HistoricalTrade>.CreateListOfSize(expected).WithValidData().Build().ForEach(x=> x.TradedAt = x.TradedAt.AddYears(-1)).ToList();
            await tradeHistoryStore.AddOrIgnoreFast(historicalTrades);

            var historicalDataPlayer = new HistoricalDataPlayer(tradeHistoryStore);
            // action
            var readHistoricalTrades = historicalDataPlayer.ReadHistoricalTrades(CurrencyPair.BTCZAR, historicalTrades.Last().TradedAt, historicalTrades.First().TradedAt,default, 2);
            // assert
            var list = readHistoricalTrades.ToList();
            list.Count.Should().Be(expected);
        }
    }
}