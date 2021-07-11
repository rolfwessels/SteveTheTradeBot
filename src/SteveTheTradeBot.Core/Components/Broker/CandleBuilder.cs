using System;
using System.Collections.Generic;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public static class CandleBuilderHelper
    {
        public static IEnumerable<CandleBuilder.Candle> ToCandleOneMinute(this IEnumerable<HistoricalTrade> trades)
        {
            return new CandleBuilder().ToCandleOneMinute(trades);
        }
    }


    public class CandleBuilder
    {
        Candle currentCandle = null;
        public Action<Candle> OnMinute { get; set; } = (z) => { };
        public void Feed(HistoricalTrade trade)
        {
            var candleDate = ToMinute(trade);
            if (currentCandle == null)
            {
                currentCandle = InitializeCandle(candleDate, trade);
            }
            else
            {
                if (currentCandle.Date == candleDate)
                {
                    UpdateCandle(currentCandle,trade);
                }
                else
                {
                    OnMinute(currentCandle);
                    currentCandle = InitializeCandle(candleDate, trade);
                }
            }
        }


        public IEnumerable<Candle> ToCandleOneMinute(IEnumerable<HistoricalTrade> trades)
        {
            Candle candle = null;
            foreach (var trade in trades)
            {
                var candleDate = ToMinute(trade);
                if (candle == null)
                {
                    candle = InitializeCandle(candleDate, trade);
                }
                else
                {
                    if (candle.Date == candleDate)
                    {
                        UpdateCandle(candle, trade);
                    }
                    else
                    {
                        yield return candle;
                        candle = InitializeCandle(candleDate, trade);
                    }
                }
            }
            if (candle != null) yield return candle;
        }

        private static void UpdateCandle(Candle candle, HistoricalTrade trade)
        {
            candle.Close = trade.Price;
            candle.High = Math.Max(candle.High, trade.Price);
            candle.Low = Math.Min(candle.Low, trade.Price);
            candle.Volume += trade.Quantity;
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