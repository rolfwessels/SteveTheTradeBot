using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class TradePersistenceFactory
    {
        private readonly DbContextOptions<TradePersistenceStoreContext> _dbContextOptions;

        public TradePersistenceFactory(string connection)
        {
            _dbContextOptions = new DbContextOptionsBuilder<TradePersistenceStoreContext>()
                .UseNpgsql(connection)
                .Options;
        }

        protected TradePersistenceFactory(DbContextOptions<TradePersistenceStoreContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public async Task<TradePersistenceStoreContext> GetTradePersistence()
        {
            var tradePersistenceStoreContext = new TradePersistenceStoreContext(_dbContextOptions);
            await tradePersistenceStoreContext.Database.EnsureCreatedAsync();
            return tradePersistenceStoreContext;
        }
    }
}