using System;
using System.Collections.Generic;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public static class TradesToCandle
    {
        public static IEnumerable<Candle> ToCandleOneMinute(this IEnumerable<HistoricalTrade> trades)
        {
            Candle candle = null;
            foreach (var historicalTrade in trades)
            {
                var candleDate = ToMinute(historicalTrade);
                if (candle == null)
                {
                    candle = InitializeCandle(candleDate, historicalTrade);
                }
                else
                {
                    if (candle.Date == candleDate)
                    {
                        candle.Close = historicalTrade.Price;
                        candle.High = Math.Max(candle.High, historicalTrade.Price);
                        candle.Low = Math.Min(candle.Low, historicalTrade.Price);
                        candle.Volume += historicalTrade.Quantity;
                    }
                    else
                    {
                        yield return candle;
                        candle = InitializeCandle(candleDate, historicalTrade);
                    }
                }
            }
            if (candle != null) yield return candle;
        }

        private static Candle InitializeCandle(DateTime candleDate, HistoricalTrade historicalTrade)
        {
            return new Candle
            {
                Date = candleDate,
                Open = historicalTrade.Price,
                Close = historicalTrade.Price,
                High = historicalTrade.Price,
                Low = historicalTrade.Price,
                Volume = historicalTrade.Quantity
            };
        }

        private static DateTime ToMinute(HistoricalTrade historicalTrade)
        {
            return new DateTime(historicalTrade.TradedAt.Year, historicalTrade.TradedAt.Month, historicalTrade.TradedAt.Day, historicalTrade.TradedAt.Hour,
                historicalTrade.TradedAt.Minute, 0, historicalTrade.TradedAt.Kind);
        }

        public class Candle : IQuote
        {
            public DateTime Date { get; set; }
            public decimal Open { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }
            public decimal Close { get; set; }
            public decimal Volume { get; set; }
        }
    }
}