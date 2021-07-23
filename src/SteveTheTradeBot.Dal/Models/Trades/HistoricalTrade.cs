using System;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class HistoricalTrade : BaseDalModelWithId
    {
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string CurrencyPair { get; set; }
        public DateTime TradedAt { get; set; }
        public string TakerSide { get; set; }
        public int SequenceId { get; set; }
        public decimal QuoteVolume { get; set; }
    }
}