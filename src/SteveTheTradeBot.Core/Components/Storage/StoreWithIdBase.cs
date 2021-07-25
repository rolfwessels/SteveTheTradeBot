using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public abstract class StoreWithIdBase<T> : StoreBase<T> where T : BaseDalModelWithId
    {
        protected StoreWithIdBase(ITradePersistenceFactory factory) : base(factory)
        {
        }

        public async Task<List<T>> FindById(params string[] ids)
        {
            await using var context = await _factory.GetTradePersistence();
            return WithFullData( DbSet(context).AsQueryable().Where(x => ids.Contains(x.Id))).ToList();
        }

        protected virtual IQueryable<T> WithFullData(IQueryable<T> query)
        {
            return query;
        }

        public async Task<int> AddOrIgnoreFast(List<T> trades)
        {
            await using var context = await _factory.GetTradePersistence();
            DbSet(context).AddRange(trades);
            try
            {
                return await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return await AddOrIgnore(trades);
            }
        }

        public async Task<int> AddOrIgnore(List<T> trades)
        {
            await using var context = await _factory.GetTradePersistence();
            var ids = trades.Select(x => x.Id).ToArray();
            var exists = context.HistoricalTrades.AsQueryable().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToList();
            DbSet(context).AddRange(trades.Where(x => !exists.Contains(x.Id)));
            return await context.SaveChangesAsync();
        }
    }
}