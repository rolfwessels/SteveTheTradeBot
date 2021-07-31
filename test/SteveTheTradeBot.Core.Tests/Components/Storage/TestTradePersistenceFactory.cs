using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Core.Components.Storage;

namespace SteveTheTradeBot.Core.Tests.Components.Storage
{
    public class TestTradePersistenceFactory : TradePersistenceFactory
    {
        private static readonly Lazy<TestTradePersistenceFactory> _instance = new Lazy<TestTradePersistenceFactory>(() => new TestTradePersistenceFactory());
        private readonly TradePersistenceFactory _tradePersistenceFactory;


        protected TestTradePersistenceFactory() : base(DbContextOptionsFor())
        {
            var lookup = "Database=";
            if (!Settings.Instance.NpgsqlConnection.Contains(lookup)) throw new Exception("Please check connection string.");
            var connection = Settings.Instance.NpgsqlConnection.Replace(lookup,"Database=Test1");
            _tradePersistenceFactory = new TradePersistenceFactory(connection);
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