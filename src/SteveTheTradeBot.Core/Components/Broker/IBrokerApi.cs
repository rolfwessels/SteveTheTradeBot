using System;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public interface IBrokerApi
    {
        Task<IdResponse> LimitOrder(LimitOrderRequest request);
        Task<IdResponse> MarketOrder(MarketOrderRequest request);
    }

    public class IdResponse
    {
        public string Id { get; set; }
    }

    public class MarketOrderRequest
    {
        public MarketOrderRequest(Side side, decimal quantity,  string pair, string customerOrderId)
        {
            Side = side;
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Value greater than 0 expected");
            Quantity = quantity;
            Pair = pair;
            CustomerOrderId = customerOrderId;
        }

        public Side Side { get; }
        public decimal Quantity { get; }
        public string Pair { get; }
        public string CustomerOrderId { get; }
    }

    public class LimitOrderRequest
    {
        public LimitOrderRequest(Side side, decimal quantity, decimal price, string pair, string customerOrderId, bool postOnly = true, TimeEnforce timeInForce = TimeEnforce.FillOrKill)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Value greater than 0 expected");
            if (price <= 0) throw new ArgumentOutOfRangeException(nameof(Price), "Value greater than 0 expected");

            Side = side;
            Quantity = quantity;
            Price = price;
            Pair = pair;
            PostOnly = postOnly;
            CustomerOrderId = customerOrderId;
            TimeInForce = timeInForce;
        }

        public Side Side { get; }
        public decimal Quantity { get; }
        public decimal Price { get; }
        public string Pair { get; }
        public bool PostOnly { get; }
        public string CustomerOrderId { get; }
        public TimeEnforce TimeInForce { get; }
    }
}



