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

        private static DbContextOptions<TradePersistenceStoreContext> DbContextOptions()
        {
            return new DbContextOptionsBuilder<TradePersistenceStoreContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
        }

        #region Singleton

        public static TestTradePersistenceFactory Instance => _instance.Value;

        #endregion

        public Task<TradePersistenceStoreContext> GetTestDb()
        {
            return _tradePersistenceFactory.GetTradePersistence();
        }
    }
}