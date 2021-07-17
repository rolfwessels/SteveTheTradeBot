using System;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class TradeResponseDto
    {
        public string Price { get; set; }
        public string Quantity { get; set; }
        public string CurrencyPair { get; set; }
        public DateTime TradedAt { get; set; }
        public string TakerSide { get; set; }
        public int SequenceId { get; set; }
        public string Id { get; set; }
        public string QuoteVolume { get; set; }
    }
}