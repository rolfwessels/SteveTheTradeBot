using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.Storage
{
    public class TradeFeedQuotesStoreTests
    {
        private TradeQuoteStore _store;

        #region Setup/Teardown

        public void Setup()
        {
            _store = new TradeQuoteStore(TestTradePersistenceFactory.UniqueDb());
        }

        #endregion

        [Test]
        public async Task FindRecentQuotes_GivenSomeData_ShouldFindRecentTrades()
        {
            // arrange
            Setup();
            var trades = Builder<HistoricalTrade>.CreateListOfSize(10).WithValidData().Build().ToCandleOneMinute();
            var tradeFeedQuotes = trades.Select(x => TradeQuote.From(x, "F", PeriodSize.OneMinute, CurrencyPair.XRPZAR)).ToList();
            await _store.AddRange(tradeFeedQuotes);
            // action
            var existingRecords = await _store.FindRecent(PeriodSize.OneMinute, tradeFeedQuotes.Max(x => x.Date), 100, CurrencyPair.XRPZAR, "F");
            // assert
            existingRecords.Should().HaveCount(9);
        }
    }
}