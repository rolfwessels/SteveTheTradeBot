using System;
using SteveTheTradeBot.Core.Components.Storage;

namespace SteveTheTradeBot.Core.Tests.Components.Storage
{
    public class TestTradePersistenceFactory: TradePersistenceFactory
    {
        private static readonly Lazy<TestTradePersistenceFactory> _instance = new Lazy<TestTradePersistenceFactory>(() => new TestTradePersistenceFactory());

        protected TestTradePersistenceFactory() : base("Host=localhost;Database=SteveTheTradeBotTestDb;Username=postgres;Password=GRES_password")
        {
        }

        #region singleton

        public static TestTradePersistenceFactory Instance
        {
            get { return _instance.Value; }
        }

        #endregion
    
    }
}