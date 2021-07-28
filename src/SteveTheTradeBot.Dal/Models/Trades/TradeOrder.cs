using System;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class TradeOrder : BaseDalModelWithGuid
    {
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public OrderStatusTypes OrderStatusType { get; set; } = OrderStatusTypes.Placed;
        public string CurrencyPair { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal PriceAtRequest { get; set; }

        public decimal RemainingQuantity { get; set; }
        public decimal OriginalQuantity { get; set; }

        public Side OrderSide { get; set; }
        public string OrderType { get; set; }
        public string BrokerOrderId { get; set; }
        public string FailedReason { get; set; } = null;
        
        public decimal Total { get; set; }
        public decimal TotalFee { get; set; }
        public string FeeCurrency { get; set; }
        public string PaidCurrency => CurrencyPair.SideOut(OrderSide);
        public decimal StopPrice { get; set; }

        public decimal SwapFeeAmount(string feeCurrency)
        {
            if (feeCurrency == CurrencyPair.QuoteCurrency()) return TotalFee;
            return TotalFee * OrderPrice;
        }
    }
}