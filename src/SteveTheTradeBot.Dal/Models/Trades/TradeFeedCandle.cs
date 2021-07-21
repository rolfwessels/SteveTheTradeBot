using System;
using System.Collections.Generic;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class TradeFeedCandle : BaseDalModel, IQuote
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
        public Dictionary<string,decimal?> Metric { get; set; } = new Dictionary<string, decimal?>();

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
}