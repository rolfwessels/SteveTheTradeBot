using System;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class MarketSummaryResponse
    {
        public string CurrencyPair { get; set; }
        public decimal AskPrice { get; set; }
        public decimal BidPrice { get; set; }
        public decimal LastTradedPrice { get; set; }
        public decimal PreviousClosePrice { get; set; }
        public decimal BaseVolume { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public DateTime Created { get; set; }
        public decimal ChangeFromPrevious { get; set; }
    }
}