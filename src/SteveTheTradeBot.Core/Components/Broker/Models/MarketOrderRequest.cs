using System;

namespace SteveTheTradeBot.Core.Components.Broker.Models
{
    public class MarketOrderRequest
    {
        public MarketOrderRequest(Side side, decimal quantity,  string pair, string customerOrderId, DateTime dateTime)
        {
            Side = side;
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Value greater than 0 expected.");
            Quantity = quantity;
            Pair = pair;
            CustomerOrderId = customerOrderId;
            DateTime = dateTime;
        }

        public Side Side { get; }
        public decimal Quantity { get; }
        public string Pair { get; }
        public string CustomerOrderId { get; }
        public DateTime DateTime { get; }
    }
}