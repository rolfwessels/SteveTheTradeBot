using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class TradePersistenceFactory 
    {
        private string _connection;
       

        public TradePersistenceFactory(string connection)
        {
            _connection = connection;
        }

        public async Task<TradePersistenceStoreContext> GetTradePersistence()
        {
            var tradePersistenceStoreContext = new TradePersistenceStoreContext(_connection);
            await tradePersistenceStoreContext.Database.EnsureCreatedAsync();
            return tradePersistenceStoreContext;
        }
    }
}