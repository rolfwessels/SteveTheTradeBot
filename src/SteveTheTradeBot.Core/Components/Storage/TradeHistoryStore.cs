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
    public interface ITradeHistoryStore
    {
        Task<(HistoricalTrade earliest, HistoricalTrade latest)> GetExistingRecords(string currencyPair);
        Task<int> AddRangeAndIgnoreDuplicates(List<HistoricalTrade> trades);
        Task<List<HistoricalTrade>> FindById(IEnumerable<string> ids);
        Task<List<HistoricalTrade>> FindByDate(DateTime @from, DateTime to, int skip=0, int take = 1000000);
        Task<List<TradeFeedCandle>> FindCandlesByDate(DateTime @from, DateTime to, PeriodSize periodSize, string feed = "valr", int skip = 0, int take = 1000000);
    }

    public class TradeHistoryStore : ITradeHistoryStore
    {   
        private readonly ITradePersistenceFactory _factory;

        public TradeHistoryStore(ITradePersistenceFactory factory)
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
            return await context.HistoricalTrades.AsQueryable().Where(x => x.CurrencyPair == CurrencyPair.BTCZAR)
                .OrderBy(x=>x.TradedAt)
                .ThenBy(x=>x.SequenceId)
                .Where(x => x.TradedAt >= from && x.TradedAt <= to)
                .Skip(skip)
                .Take(take).ToListAsync();
        }

        public async Task<List<TradeFeedCandle>> FindCandlesByDate(DateTime @from, DateTime to, PeriodSize periodSize, string feed = "valr", int skip = 0, int take = 1000000)
        {
            var context = await _factory.GetTradePersistence();
            return await context.TradeFeedCandles.AsQueryable()
                .Where(x=> x.Feed == feed && x.CurrencyPair == CurrencyPair.BTCZAR && x.PeriodSize == periodSize  && x.Date >= from && x.Date <= to)
                .OrderBy(x => x.Date)
                .Skip(skip)
                .Take(take).ToListAsync();
        }
    }
}