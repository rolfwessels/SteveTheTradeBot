using System;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class HistoricalTrade
    {
        public string Id { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string CurrencyPair { get; set; }
        public DateTime TradedAt { get; set; }
        public string TakerSide { get; set; }
        public int SequenceId { get; set; }
        public decimal QuoteVolume { get; set; }
    }

    
}