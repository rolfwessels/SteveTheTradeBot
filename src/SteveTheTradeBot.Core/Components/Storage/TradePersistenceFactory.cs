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
            _dbContextOptions = new DbContextOptionsBuilder<TradePersistenceStoreContext>()
                .UseNpgsql(connection)
                .Options;
        }

        public TradePersistenceFactory(DbContextOptions<TradePersistenceStoreContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public async Task<TradePersistenceStoreContext> GetTradePersistence()
        {
            var tradePersistenceStoreContext = new TradePersistenceStoreContext(_dbContextOptions);
            if (!_isInitialized)
            {
                lock (_locker)
                {
                    tradePersistenceStoreContext.Database.EnsureCreated();
                    tradePersistenceStoreContext.Database.Migrate();
                    _isInitialized = true;
                }
            }

            return tradePersistenceStoreContext;
        }
    }
}