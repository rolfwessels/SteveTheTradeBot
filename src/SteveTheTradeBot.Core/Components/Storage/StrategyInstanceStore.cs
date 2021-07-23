using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Persistence;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class StrategyInstanceStore : StoreWithIdBase<StrategyInstance>
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
                .Include(x => x.Trades)
                .FirstOrDefault(x => x.Reference == reference);
            if (found != null)
            {
                DbSet(context).Remove(found);
                await context.SaveChangesAsync();
            }
        }
    }
}
