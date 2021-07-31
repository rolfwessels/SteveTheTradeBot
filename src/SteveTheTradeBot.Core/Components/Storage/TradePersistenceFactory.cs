using System;
using System.Threading;
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
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);
        private bool _isInitialized =false;

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
                _semaphore.Wait();
                try
                {
                    if (!_isInitialized && context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
                    {
                        await context.Database.MigrateAsync();
                    }
                    _isInitialized = true;

                }
                finally
                {
                    _semaphore.Release();
                }
                
            }

            return context;
        }
    }
}