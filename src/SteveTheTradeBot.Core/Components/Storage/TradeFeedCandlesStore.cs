using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Base;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Persistence;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public abstract class StoreBase<T> : IRepository<T> where T : BaseDalModel 
    {
        
        protected readonly ITradePersistenceFactory _factory;

        protected StoreBase(ITradePersistenceFactory factory)
        {
            _factory = factory;
        }

        

        protected abstract DbSet<T> DbSet(TradePersistenceStoreContext context);

        public async Task<int> Remove(T foundCandle)
        {
            await using var context = await _factory.GetTradePersistence();
            DbSet(context).Remove(foundCandle);
            return context.SaveChanges();
        }

        #region Implementation of IRepository<T>

        public IQueryable<T> Query()
        {
            var context = _factory.GetTradePersistence().Result;
            return DbSet(context).AsQueryable();
        }

        public async Task<T> Add(T entity)
        {
            await using var context = await _factory.GetTradePersistence();
            DbSet(context).Add(entity);
            context.SaveChanges();
            return entity;
        }

        public async Task<IEnumerable<T>> AddRange(IEnumerable<T> entities)
        {
            await using var context = await _factory.GetTradePersistence();
            var baseDalModels = entities as T[] ?? entities.ToArray();
            DbSet(context).AddRange(baseDalModels);
            context.SaveChanges();
            return baseDalModels;
        }

        public Task<T> Update(Expression<Func<T, bool>> filter, T entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Remove(Expression<Func<T, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> Find(Expression<Func<T, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<T> FindOne(Expression<Func<T, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public async Task<long> Count()
        {
            await using var context = await _factory.GetTradePersistence();
            return DbSet(context).Count();
        }

        public Task<long> Count(Expression<Func<T, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<long> UpdateMany(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            throw new NotImplementedException();
        }

        public Task<long> UpdateOne(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            throw new NotImplementedException();
        }

        public Task<long> Upsert(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            throw new NotImplementedException();
        }

        public Task<T> FindOneAndUpdate(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd, bool isUpsert = false)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

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

        public async Task<int> UpdateFeed(IEnumerable<KeyValuePair<DateTime, Dictionary<string, decimal?>>> store,
            string feed, string currencyPair,
            PeriodSize periodSize, string candleName)
        {
            
            await using var context = await _factory.GetTradePersistence();
            var keyValuePairs = store.ToDictionary(x=>x.Key,x=>x.Value);
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
            return await context.SaveChangesAsync();
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

    public interface ITradeFeedCandlesStore
    {
        Task<TradeFeedCandle> FindLatestCandle(string feed, string currencyPair, PeriodSize periodSize);
        Task<int> Remove(TradeFeedCandle foundCandle);
        IEnumerable<TradeFeedCandle> FindAllBetween( DateTime fromDate,  DateTime toDate, string feed,
            string currencyPair, PeriodSize periodSize, int batchSize = 1000);

        Task<int> AddRange(List<TradeFeedCandle> feedCandles);
        Task<int> UpdateFeed(IEnumerable<KeyValuePair<DateTime, Dictionary<string, decimal?>>> store, string feed,
            string currencyPair, PeriodSize periodSize,
            string candleName);
    }
}