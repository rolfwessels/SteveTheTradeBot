using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Persistence;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class StrategyInstanceStore : StoreWithIdBase<StrategyInstance>, IStrategyInstanceStore
    {
        public StrategyInstanceStore(ITradePersistenceFactory factory) : base(factory)
        {
        }

        #region Overrides of StoreBase<StrategyTrade>

        protected override DbSet<StrategyInstance> DbSet(TradePersistenceStoreContext context)
        {
            return context.Strategies;
        }

        #endregion


        public async Task RemoveByReference(string reference)
        {
            await using var context = await _factory.GetTradePersistence();
            var found = DbSet(context).AsQueryable()
                .Where(x => x.Reference == reference && (x.IsBackTest || !x.IsActive))
                .Include(x => x.Trades)
                .FirstOrDefault();
            if (found != null)
            {
                DbSet(context).Remove(found);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<StrategyInstance>> FindActiveStrategies()
        {
            await using var context = await _factory.GetTradePersistence();
            return DbSet(context).AsQueryable()
                .Where(x =>  x.IsActive && !x.IsBackTest).ToList();
        }

        public async Task<T> EnsureUpdate<T>(string id, Func<StrategyInstance,Task<T>> action)
        {
            await using var context = await _factory.GetTradePersistence();
            var strategyInstance = WithFullData(DbSet(context).AsQueryable().Where(x=>x.Id == id)).FirstOrDefault().ExistsOrThrow(id);
            try
            {
                var result = await action(strategyInstance);
                return result;
            }
            finally
            {
                DbSet(context).Update(strategyInstance);
                context.SaveChanges();
            }
        }

        #region Overrides of StoreWithIdBase<StrategyInstance>

        protected override IQueryable<StrategyInstance> WithFullData(IQueryable<StrategyInstance> query)
        {
            return base.WithFullData(query).Include(r => r.Trades);
        }

        #endregion
    }
}
