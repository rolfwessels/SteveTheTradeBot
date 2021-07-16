using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public interface IBrokerApi
    { 
        Task<OrderStatusResponse> LimitOrder(LimitOrderRequest request);
        Task<OrderStatusResponse> MarketOrder(MarketOrderRequest request);
        Task<IdResponse> StopLimitOrder(StopLimitOrderRequest request);
        Task<SimpleOrderStatusResponse> Order(SimpleOrderRequest simpleOrderRequest);
    }

    
}



