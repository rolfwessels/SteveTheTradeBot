using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Core.Components.Storage;

namespace SteveTheTradeBot.Core.Tests.Components.Storage
{
    public class TestTradePersistenceFactory: TradePersistenceFactory
    {
        private static readonly Lazy<TestTradePersistenceFactory> _instance = new Lazy<TestTradePersistenceFactory>(() => new TestTradePersistenceFactory());
        private TradePersistenceFactory _tradePersistenceFactory;


        protected TestTradePersistenceFactory() : base(DbContextOptions())
        {
            _tradePersistenceFactory = new TradePersistenceFactory("Host=localhost;Database=SteveTheTradeBotTests;Username=postgres;Password=GRES_password");
        }

        private static DbContextOptions<TradePersistenceStoreContext> DbContextOptions(string databaseName = "TestDb")
        {
            return new DbContextOptionsBuilder<TradePersistenceStoreContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
        }

        #region Singleton

        public static TestTradePersistenceFactory Instance => _instance.Value;

        #endregion

        public Task<TradePersistenceStoreContext> GetTestDb()
        {
            return _tradePersistenceFactory.GetTradePersistence();
        }

        public static ITradePersistenceFactory UniqueDb()
        {
            return new TradePersistenceFactory(DbContextOptions(Guid.NewGuid().ToString("n")));
        }
    }
}