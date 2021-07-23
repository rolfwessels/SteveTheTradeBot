using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class TradeHistoryStore : StoreWithIdBase<HistoricalTrade>, ITradeHistoryStore
    {   
        private readonly ITradePersistenceFactory _factory;

        public TradeHistoryStore(ITradePersistenceFactory factory) :base(factory)
        {
            _factory = factory;
        }


        public async Task<(HistoricalTrade earliest, HistoricalTrade latest)> GetExistingRecords(string currencyPair)
        {
            var context = await _factory.GetTradePersistence();
            var earliest = context.HistoricalTrades.AsQueryable().Where(x=> x.CurrencyPair == currencyPair).OrderBy(x => x.TradedAt).Take(1).FirstOrDefault();
            var latest = context.HistoricalTrades.AsQueryable().Where(x => x.CurrencyPair == currencyPair).OrderByDescending(x => x.TradedAt).Take(1).FirstOrDefault();
            return (earliest, latest);
        }

        public Task<int> AddRangeAndIgnoreDuplicates(List<HistoricalTrade> trades)
        {
            return AddOrIgnoreFast(trades);
        }

        public async Task<List<HistoricalTrade>> FindByDate(string currencyPair, DateTime @from, DateTime to,
            int skip = 0, int take = 1000000)
        {
            var context = await _factory.GetTradePersistence();
            return await context.HistoricalTrades.AsQueryable().Where(x => x.CurrencyPair == currencyPair)
                .OrderBy(x=>x.TradedAt)
                .ThenBy(x=>x.SequenceId)
                .Where(x => x.TradedAt >= from && x.TradedAt <= to)
                .Skip(skip)
                .Take(take).ToListAsync();
        }

        public async Task<List<TradeFeedCandle>> FindCandlesByDate(string currencyPair, DateTime @from, DateTime to, PeriodSize periodSize, string feed = "valr", int skip = 0, int take = 1000000)
        {
            var context = await _factory.GetTradePersistence();
            return await context.TradeFeedCandles.AsQueryable()
                .Where(x=> x.Feed == feed && x.CurrencyPair == currencyPair && x.PeriodSize == periodSize  && x.Date >= from && x.Date <= to)
                .OrderBy(x => x.Date)
                .Skip(skip)
                .Take(take).ToListAsync();
        }


        public async Task<List<TradeFeedCandle>> FindRecentCandles(PeriodSize periodSize, DateTime beforeDate, int take, string feed = "valr")
        {
            var context = await _factory.GetTradePersistence();
            return await context.TradeFeedCandles.AsQueryable()
                .Where(x => x.Feed == feed && x.CurrencyPair == CurrencyPair.BTCZAR && x.PeriodSize == periodSize && x.Date < beforeDate)
                .OrderByDescending(x => x.Date)
                .Take(take)
                .ToListAsync();
        }

        #region Overrides of StoreBase<HistoricalTrade>

        protected override DbSet<HistoricalTrade> DbSet(TradePersistenceStoreContext context)
        {
            return context.HistoricalTrades;
        }

        #endregion
    }
}