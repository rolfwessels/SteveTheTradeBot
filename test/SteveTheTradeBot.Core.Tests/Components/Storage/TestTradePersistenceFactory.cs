using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Core.Components.Storage;

namespace SteveTheTradeBot.Core.Tests.Components.Storage
{
    public class TestTradePersistenceFactory: TradePersistenceFactory
    {
        private static readonly Lazy<TestTradePersistenceFactory> _instance = new Lazy<TestTradePersistenceFactory>(() => new TestTradePersistenceFactory());
        private readonly TradePersistenceFactory _tradePersistenceFactory;


        protected TestTradePersistenceFactory() : base(DbContextOptionsFor())
        {
            _tradePersistenceFactory = new TradePersistenceFactory("Host=localhost;Database=SteveTheTradeBotTests;Username=postgres;Password=GRES_password;Port=15432");
        }

        private static DbContextOptions<TradePersistenceStoreContext> DbContextOptionsFor(string databaseName = "TestDb")
        {
            return new DbContextOptionsBuilder<TradePersistenceStoreContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
        }

        #region Singleton

        public static TestTradePersistenceFactory InMemoryDb => _instance.Value;
        public static TradePersistenceFactory PostgresTest => _instance.Value._tradePersistenceFactory;

        #endregion

        protected Task<TradePersistenceStoreContext> GetTestDb()
        {
            return _tradePersistenceFactory.GetTradePersistence();
        }

        public static ITradePersistenceFactory UniqueDb()
        {
            return new TradePersistenceFactory(DbContextOptionsFor(Guid.NewGuid().ToString("n")));
        }

        public static ITradePersistenceFactory RealDb()
        {
            return new TradePersistenceFactory(Settings.Instance.NpgsqlConnection);
        }
    }
}