using System;
using System.Collections.Generic;
using StackExchange.Redis;
using SteveTheTradeBot.Core.Components.Broker.Models;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class OrderBookResponse
    {
        public List<OrderValue> Asks { get; set; }
        public List<OrderValue> Bids { get; set; }
        public DateTime LastChange { get; set; }

        public class OrderValue
        {
            public Side Side { get; set; }
            public Decimal Quantity { get; set; }
            public Decimal Price { get; set; }
            public string CurrencyPair { get; set; }
            public int OrderCount { get; set; }
        }
    }
}