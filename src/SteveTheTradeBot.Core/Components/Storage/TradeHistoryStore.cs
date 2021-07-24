using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class TradeHistoryStore : StoreWithIdBase<HistoricalTrade>, ITradeHistoryStore
    {   
       
        public TradeHistoryStore(ITradePersistenceFactory factory) :base(factory)
        {
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



        #region Overrides of StoreBase<HistoricalTrade>

        protected override DbSet<HistoricalTrade> DbSet(TradePersistenceStoreContext context)
        {
            return context.HistoricalTrades;
        }

        #endregion
    }
}