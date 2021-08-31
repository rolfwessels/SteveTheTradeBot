using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public interface IBrokerApi
    { 
        Task<OrderHistorySummaryResponse> MarketOrder(SimpleOrderRequest request);
        Task<IdResponse> StopLimitOrder(StopLimitOrderRequest request);
        Task<SimpleOrderStatusResponse> Order(SimpleOrderRequest simpleOrderRequest);
        Task CancelOrder(string brokerOrderId, string pair);
        Task<bool> SyncOrderStatus(StrategyInstance instance, StrategyContext strategyContext);
    }
}



