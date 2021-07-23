using System;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker.Models
{
    public class StopLimitOrderRequest
    {
        public StopLimitOrderRequest(Side side, decimal quantity, decimal price, string pair, string customerOrderId, TimeEnforce timeInForce, decimal stopPrice, Types type)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Value greater than 0 expected.");
            if (type == Types.StopLossLimit && StopPrice >= price) throw new ArgumentOutOfRangeException(nameof(quantity), "Stop prices expected to be lower than price for stop loss limit.");
            if (type == Types.TakeProfitLimit && StopPrice <= price) throw new ArgumentOutOfRangeException(nameof(quantity), "Stop prices expected to be higher than price for take profit limit.");
            if (price <= 0) throw new ArgumentOutOfRangeException(nameof(Price), "Value greater than 0 expected.");
            Side = side;
            Quantity = quantity;
            Price = price;
            Pair = pair;
            CustomerOrderId = customerOrderId;
            TimeInForce = timeInForce;
            StopPrice = stopPrice;
            Type = type;
        }

        public Side Side { get; }
        public decimal Quantity { get;  }
        public decimal Price { get;  }
        public string Pair { get;  }
        public string CustomerOrderId { get;  }
        public TimeEnforce TimeInForce { get; }
        public decimal StopPrice { get;  }
        public Types Type { get; }

        public enum Types
        {
            TakeProfitLimit,
            StopLossLimit
        }
    }
}