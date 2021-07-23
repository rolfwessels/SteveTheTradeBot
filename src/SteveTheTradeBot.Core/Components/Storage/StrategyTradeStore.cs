using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{


    public class StrategyTradeStore : StoreWithIdBase<StrategyTrade>
    {
        public StrategyTradeStore(ITradePersistenceFactory factory) : base(factory)
        {
        }

        #region Overrides of StoreBase<StrategyTrade>

        protected override DbSet<StrategyTrade> DbSet(TradePersistenceStoreContext context)
        {
            return context.Trades;
        }

        #endregion
    }
}