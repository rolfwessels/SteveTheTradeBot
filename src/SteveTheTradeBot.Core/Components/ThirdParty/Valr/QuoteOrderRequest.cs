using System;
using System.Globalization;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class SimpleOrderRequest
    {
        public string PayInCurrency { get; set; }
        public decimal PayAmount { get; set; }
        public Side Side { get; set; }
        public string CustomerOrderId { get; set; }
        public DateTime RequestDate { get; set; }
        public string CurrencyPair { get; set; }


        public static SimpleOrderRequest From(Side side, decimal amount, string payIn, DateTime requestDate,
             string customerOrderId, string currencyPair)
        {
            return new SimpleOrderRequest { Side = side , PayAmount = amount, PayInCurrency = payIn, CustomerOrderId = customerOrderId, RequestDate = requestDate , CurrencyPair = currencyPair };
        }
    }
}