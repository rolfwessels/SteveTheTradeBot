using System;
using System.Collections.Generic;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public static class QuoteBuilderHelper
    {
        public static IEnumerable<QuoteBuilder.QuoteDto> ToCandleOneMinute(this IEnumerable<HistoricalTrade> trades)
        {
            return new QuoteBuilder().ToOneMinuteQuote(trades);
        }
    }


    public class QuoteBuilder
    {
        QuoteDto _currentQuote = null;
        public Action<QuoteDto> OnMinute { get; set; } = (z) => { };
        public void Feed(HistoricalTrade trade)
        {
            var candleDate = ToMinute(trade);
            if (_currentQuote == null)
            {
                _currentQuote = InitializeCandle(candleDate, trade);
            }
            else
            {
                if (_currentQuote.Date == candleDate)
                {
                    UpdateQuote(_currentQuote,trade);
                }
                else
                {
                    OnMinute(_currentQuote);
                    _currentQuote = InitializeCandle(candleDate, trade);
                }
            }
        }


        public IEnumerable<QuoteDto> ToOneMinuteQuote(IEnumerable<HistoricalTrade> trades)
        {
            QuoteDto quoteDto = null;
            foreach (var trade in trades)
            {
                var candleDate = ToMinute(trade);
                if (quoteDto == null)
                {
                    quoteDto = InitializeCandle(candleDate, trade);
                }
                else
                {
                    if (quoteDto.Date == candleDate)
                    {
                        UpdateQuote(quoteDto, trade);
                    }
                    else
                    {
                        yield return quoteDto;
                        quoteDto = InitializeCandle(candleDate, trade);
                    }
                }
            }
            if (quoteDto != null) yield return quoteDto;
        }

        private static void UpdateQuote(QuoteDto quoteDto, HistoricalTrade trade)
        {
            quoteDto.Close = trade.Price;
            quoteDto.High = Math.Max(quoteDto.High, trade.Price);
            quoteDto.Low = Math.Min(quoteDto.Low, trade.Price);
            quoteDto.Volume += trade.Quantity;
        }

        private static QuoteDto InitializeCandle(DateTime candleDate, HistoricalTrade historicalTrade)
        {
            return new QuoteDto
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

        public class QuoteDto : IQuote
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