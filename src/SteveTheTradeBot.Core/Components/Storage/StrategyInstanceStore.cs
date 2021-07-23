using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Trades;

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
    }
}