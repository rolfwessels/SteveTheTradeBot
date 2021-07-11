using System;
using System.Collections.Generic;
using Skender.Stock.Indicators;

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

    public class TradeFeedCandle : IQuote
    {
        public string Feed { get; set; }
        public PeriodSize PeriodSize { get; set; }
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }

        public static TradeFeedCandle From(IQuote trade, string feed , PeriodSize periodSize)
        {
            return new TradeFeedCandle {
                 Feed = feed,
                 PeriodSize = periodSize,
                 Date = trade.Date,
                 Open = trade.Open,
                 High = trade.High,
                 Low = trade.Low,
                 Close = trade.Close,
                 Volume = trade.Volume,
            };
        }
    }
}