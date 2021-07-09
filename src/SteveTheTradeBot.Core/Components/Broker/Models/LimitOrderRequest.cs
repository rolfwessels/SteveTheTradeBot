using System;

namespace SteveTheTradeBot.Core.Components.Broker.Models
{
    public class LimitOrderRequest
    {
        public LimitOrderRequest(Side side, decimal quantity, decimal price, string pair, string customerOrderId, bool postOnly = true, TimeEnforce timeInForce = TimeEnforce.FillOrKill)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Value greater than 0 expected.");
            if (price <= 0) throw new ArgumentOutOfRangeException(nameof(Price), "Value greater than 0 expected.");

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