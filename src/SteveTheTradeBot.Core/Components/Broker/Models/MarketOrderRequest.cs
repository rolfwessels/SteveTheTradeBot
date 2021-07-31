using System;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker.Models
{
    public class MarketOrderRequest
    {
        public MarketOrderRequest(Side side, decimal? quoteAmount, decimal? baseAmount, string pair, string customerOrderId, DateTime dateTime)
        {
            Side = side;
            if (QuoteAmount <= 0 || BaseAmount <= 0) throw new ArgumentOutOfRangeException(nameof(quoteAmount), "Value greater than 0 expected.");
            QuoteAmount = quoteAmount;
            BaseAmount = baseAmount;
            Pair = pair;
            CustomerOrderId = customerOrderId;
            DateTime = dateTime;
        }

        public Side Side { get; }
        public decimal? QuoteAmount { get; }
        public decimal? BaseAmount { get; }
        public string Pair { get; }
        public string CustomerOrderId { get; }
        public DateTime DateTime { get; }
    }
}