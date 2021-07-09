using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.Broker.Models;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public interface IBrokerApi
    {
        Task<IdResponse> LimitOrder(LimitOrderRequest request);
        Task<IdResponse> MarketOrder(MarketOrderRequest request);
        Task<IdResponse> StopLimitOrder(StopLimitOrderRequest request);
    }
}



