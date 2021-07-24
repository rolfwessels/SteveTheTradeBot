using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public interface ITradePersistenceFactory
    {
        Task<TradePersistenceStoreContext> GetTradePersistence();
    }

    public class TradePersistenceFactory : ITradePersistenceFactory
    {
        private readonly DbContextOptions<TradePersistenceStoreContext> _dbContextOptions;
        private object _locker = new object();
        private bool _isInitialized = false;
        public TradePersistenceFactory(string connection)
        {
            _dbContextOptions = DbContextOptions(connection);
        }

        public static DbContextOptions<TradePersistenceStoreContext> DbContextOptions(string connection)
        {
            return new DbContextOptionsBuilder<TradePersistenceStoreContext>()
                .UseNpgsql(connection)
                .Options;
        }

        public TradePersistenceFactory(DbContextOptions<TradePersistenceStoreContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public async Task<TradePersistenceStoreContext> GetTradePersistence()
        {
            var context = new TradePersistenceStoreContext(_dbContextOptions);
            if (!_isInitialized)
            {
                //await context.Database.EnsureCreatedAsync();
                if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
                {
                    await context.Database.MigrateAsync();
                }
                _isInitialized = true;
            }

            return context;
        }
    }
}