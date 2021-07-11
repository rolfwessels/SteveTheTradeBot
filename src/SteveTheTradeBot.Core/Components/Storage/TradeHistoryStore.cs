using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public interface ITradeHistoryStore
    {
        Task<(HistoricalTrade earliest, HistoricalTrade latest)> GetExistingRecords();
        Task<int> AddRangeAndIgnoreDuplicates(List<HistoricalTrade> trades);
    }

    public class TradeHistoryStore : ITradeHistoryStore
    {
        private readonly ITradePersistenceFactory _factory;

        public TradeHistoryStore(ITradePersistenceFactory factory)
        {
            _factory = factory;
        }


        public async Task<(HistoricalTrade earliest, HistoricalTrade latest)> GetExistingRecords()
        {
            var context = await _factory.GetTradePersistence();
            var earliest = context.HistoricalTrades.AsQueryable().OrderBy(x => x.TradedAt).Take(1).FirstOrDefault();
            var latest = context.HistoricalTrades.AsQueryable().OrderByDescending(x => x.TradedAt).Take(1).FirstOrDefault();
            return (earliest, latest);
        }

        public async Task<int> AddRangeAndIgnoreDuplicates(List<HistoricalTrade> trades)
        {
            var context = await _factory.GetTradePersistence();
            var historicalTrades = trades;
            context.HistoricalTrades.AddRange(historicalTrades);
            try
            {
                return await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                var ids = trades.Select(x => x.Id).ToArray();
                var exists = context.HistoricalTrades.AsQueryable().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToList();
                context = await _factory.GetTradePersistence();
                context.HistoricalTrades.AddRange(historicalTrades.Where(x => !exists.Contains(x.Id)));
                return await context.SaveChangesAsync();
            }
        }

        public async Task<List<HistoricalTrade>> FindById(IEnumerable<string> ids)
        {
            var context = await _factory.GetTradePersistence();
            return context.HistoricalTrades.AsQueryable().Where(x => ids.Contains(x.Id)).ToList();
        }

        public async Task<List<HistoricalTrade>> FindByDate(DateTime @from, DateTime to, int skip=0, int take = 1000000)
        {
            var context = await _factory.GetTradePersistence();
            return await context.HistoricalTrades.AsQueryable()
                .OrderBy(x=>x.TradedAt)
                .Where(x => x.TradedAt >= from && x.TradedAt <= to)
                .Skip(skip)
                .Take(take).ToListAsync();
        }
    }
}