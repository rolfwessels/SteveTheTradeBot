using System;
using Skender.Stock.Indicators;

namespace SteveTheTradeBot.Dal.Models.Trades
{
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
        public string CurrencyPair { get; set; }

        public static TradeFeedCandle From(IQuote trade, string feed , PeriodSize periodSize, string currencyPair)
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
                CurrencyPair = currencyPair
            };
        }
    }

    public class DynamicPlotter 
    {
        public string Feed { get; set; }
        public string Label { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }
}