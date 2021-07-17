using System;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class OrderHistorySummaryResponse
    {
        public string OrderId { get; set; } = Gu.Id();
        public string CustomerOrderId { get; set; }
        public string OrderStatusType { get; set; }
        public string CurrencyPair { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal OriginalQuantity { get; set; }
        public decimal Total { get; set; }
        public decimal TotalFee { get; set; }
        public string FeeCurrency { get; set; }
        public Side OrderSide { get; set; }
        public string OrderType { get; set; }
        public string FailedReason { get; set; } = null;
        public DateTime OrderUpdatedAt { get; set; } = DateTime.Now;
        public DateTime OrderCreatedAt { get; set; } = DateTime.Now;
    }
}