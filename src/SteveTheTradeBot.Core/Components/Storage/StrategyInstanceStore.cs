using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Persistence;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public interface IStrategyInstanceStore : IRepository<StrategyInstance>
    {
        Task RemoveByReference(string reference);
        Task<List<StrategyInstance>> FindActiveStrategies();
        Task<StrategyInstance> Update(StrategyInstance botDataStrategyInstance);
    }

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
    }
}
