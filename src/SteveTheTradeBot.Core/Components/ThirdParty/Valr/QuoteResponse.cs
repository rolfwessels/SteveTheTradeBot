using System;
using System.Collections.Generic;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class QuoteResponse
    {
        public string CurrencyPair { get; set; }
        public decimal PayAmount { get; set; }
        public decimal ReceiveAmount { get; set; }
        public decimal Fee { get; set; }
        public string FeeCurrency { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Id { get; set; }
        public List<ToMatch> OrdersToMatch { get; set; }
        public class ToMatch
        {
            public decimal Price { get; set; }
            public decimal Quantity { get; set; }
        }
    }
}