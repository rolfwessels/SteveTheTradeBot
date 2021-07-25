using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class TradeFeedCandlesStore : StoreBase<TradeFeedCandle>, ITradeFeedCandlesStore
    {
        #region Implementation of ITradeFeedCandlesStore

        public async Task<TradeFeedCandle> FindLatestCandle(string feed, string currencyPair, PeriodSize periodSize)
        {
            await using var context = await _factory.GetTradePersistence();
            return DbSet(context).AsQueryable()
                .Where(x => x.Feed == feed && x.CurrencyPair == currencyPair && x.PeriodSize == periodSize)
                .OrderByDescending(x => x.Date).Take(1).FirstOrDefault();
        }

       
        public async Task<int> AddRange(List<TradeFeedCandle> feedCandles)
        {
            await using var context = await _factory.GetTradePersistence();
            context.TradeFeedCandles.AddRange(feedCandles);
            return await context.SaveChangesAsync();
        }

        public async Task<List<TradeFeedCandle>> UpdateFeed(
            IEnumerable<KeyValuePair<DateTime, Dictionary<string, decimal?>>> store,
            string feed, string currencyPair,
            PeriodSize periodSize)
        {
            
            await using var context = await _factory.GetTradePersistence();
            var keyValuePairs = store.ToDictionary(x=>x.Key.ToUniversalTime(),x=>x.Value);
            var dateTimes = keyValuePairs.Keys;
            
            var candles = DbSet(context).AsQueryable()
                .Where(x => x.Feed == feed && x.CurrencyPair == currencyPair && x.PeriodSize == periodSize && dateTimes.Contains(x.Date))
                .OrderByDescending(x => x.Date)
                .ToList();
            foreach (var candle in candles)
            {
                if (candle.Metric == null) candle.Metric = new Dictionary<string, decimal?>();
                candle.Metric.AddOrReplace(keyValuePairs[candle.Date]);
            }
            DbSet(context).UpdateRange(candles);
            await context.SaveChangesAsync();
            return candles;
        }

        public async Task<List<TradeFeedCandle>> FindBefore(DateTime startDate, string feed, string currencyPair,
            PeriodSize periodSize,  int take)
        {
            await using var context = await _factory.GetTradePersistence();
            var tradeFeedCandles = context.TradeFeedCandles.AsNoTracking().AsQueryable()
                .Where(x => x.Feed == feed && 
                            x.CurrencyPair == currencyPair 
                            && x.PeriodSize == periodSize &&
                            x.Date < startDate)
                .OrderByDescending(x => x.Date)
                .Take(take)
                .ToList();
            return tradeFeedCandles;
        }


        public async Task<List<TradeFeedCandle>> FindCandlesByDate(string currencyPair, DateTime @from, DateTime to, PeriodSize periodSize, string feed = "valr", int skip = 0, int take = 1000000)
        {
            await using var context = await _factory.GetTradePersistence();
            return await context.TradeFeedCandles.AsQueryable()
                .Where(x => x.Feed == feed && x.CurrencyPair == currencyPair && x.PeriodSize == periodSize && x.Date >= from && x.Date <= to)
                .OrderBy(x => x.Date)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<List<TradeFeedCandle>> FindRecentCandles(PeriodSize periodSize, DateTime beforeDate, int take, string currencyPair, string feed)
        {
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            await using var context = await _factory.GetTradePersistence();
            return await context.TradeFeedCandles.AsQueryable()
                .Where(x => x.Feed == feed && x.CurrencyPair == currencyPair && x.PeriodSize == periodSize && x.Date < beforeDate)
                .OrderByDescending(x => x.Date)
                .Take(take)
                .ToListAsync();
        }

        public IEnumerable<TradeFeedCandle> FindAllBetween( DateTime fromDate,  DateTime toDate, string feed,
            string currencyPair, PeriodSize periodSize, int batchSize = 1000)
        {
            using var context = _factory.GetTradePersistence().Result;
            var skip = 0;
            var counter = 0;
            do
            {   
                var tradeFeedCandles = context.TradeFeedCandles.AsQueryable()
                    .Where(x => x.Feed == feed && x.CurrencyPair == currencyPair && x.PeriodSize == periodSize &&
                                x.Date >= fromDate && x.Date <= toDate)
                    .OrderBy(x=>x.Date)
                    .Skip(skip)
                    .Take(batchSize);
                
                skip += batchSize;
                counter = 0;
                foreach (var historicalTrade in tradeFeedCandles)
                {
                    yield return historicalTrade;
                    counter++;
                }

            } while (counter != 0);
            
        }

        #endregion

        public TradeFeedCandlesStore(ITradePersistenceFactory factory) : base(factory)
        {
        }

        #region Overrides of StoreBase<TradeFeedCandle>

        protected override DbSet<TradeFeedCandle> DbSet(TradePersistenceStoreContext context)
        {
            return context.TradeFeedCandles;
        }

        #endregion
    }
}