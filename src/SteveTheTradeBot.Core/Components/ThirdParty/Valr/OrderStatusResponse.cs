using System;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class OrderStatusResponse
    {
        public string OrderId { get; set; } = Gu.Id();
        public string OrderStatusType { get; set; }
        public string CurrencyPair { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal OriginalQuantity { get; set; }
        public Side OrderSide { get; set; }
        public string OrderType { get; set; }
        public string CustomerOrderId { get; set; }
        public string FailedReason { get; set; } = null;
        public DateTime OrderUpdatedAt { get; set; } = DateTime.Now;
        public DateTime OrderCreatedAt { get; set; } = DateTime.Now;
    }
}