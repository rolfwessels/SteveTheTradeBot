using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public class BrokerFactory
    {
        private ValrSettings _valrSettings;

        public BrokerFactory(ValrSettings valrSettings)
        {
            _valrSettings = valrSettings;
        }

        public IBrokerApi GetBroker()
        {
            if (_valrSettings.ApiName == "ValrBrokerApi")
            {
                return new ValrBrokerApi(_valrSettings.ApiKey, _valrSettings.Secret);
            }
            return new ValrBrokerPaperTradingApi(_valrSettings.ApiKey, _valrSettings.Secret,
                Messenger.Default);
        }
    }
}