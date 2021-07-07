using SteveTheTradeBot.Sdk.RestApi;

namespace SteveTheTradeBot.Sdk
{
    public class ConnectionFactory
    {
        private readonly string _urlBase;

        public ConnectionFactory(string urlBase)
        {
            _urlBase = urlBase;
        }

        public ISteveTheTradeBotClient GetConnection()
        {
            return new SteveTheTradeBotClient(_urlBase);
        }
    }
}