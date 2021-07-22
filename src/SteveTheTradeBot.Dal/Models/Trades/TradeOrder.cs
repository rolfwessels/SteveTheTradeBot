using System;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class TradeOrder : BaseDalModelWithGuid
    {
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public OrderStatusTypes OrderStatusType { get; set; } = OrderStatusTypes.Placed;
        public string CurrencyPair { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal OriginalQuantity { get; set; }
        public Side OrderSide { get; set; }
        public string OrderType { get; set; }
        public string BrokerOrderId { get; set; }
        public string FailedReason { get; set; } = null;
        public decimal PriceAtRequest { get; set; }
        public decimal OutQuantity { get; set; }
        public string OutCurrency { get; set; }
        public decimal FeeAmount { get; set; }
        public string FeeCurrency { get; set; }

        public decimal SwapFeeAmount(string feeCurrency)
        {
            if (feeCurrency == FeeCurrency) return FeeAmount;
            return FeeAmount * OrderPrice;
        }
    }
}