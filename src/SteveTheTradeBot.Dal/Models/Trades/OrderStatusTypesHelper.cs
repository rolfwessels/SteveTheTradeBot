using System;

namespace SteveTheTradeBot.Core.Components.Broker.Models
{
    public static class OrderStatusTypesHelper
    {
        public static OrderStatusTypes ToOrderStatus(string statusType)
        {
            switch (statusType.ToLower())
            {
                case "filled":
                    return OrderStatusTypes.Filled;
                case "placed":
                    return OrderStatusTypes.Placed; 
                case "partially filled":
                    return OrderStatusTypes.PartiallyFilled;
                case "failed":
                    return OrderStatusTypes.Failed;
                default:
                    throw new ArgumentOutOfRangeException(statusType,$"Status type `{statusType}` could not be mapped.");
            }
        }
    }
}