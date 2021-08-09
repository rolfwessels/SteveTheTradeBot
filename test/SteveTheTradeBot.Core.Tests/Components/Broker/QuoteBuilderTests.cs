using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Components.Broker
{
    public class CandleBuilderTests
    {
        [Test]
        public void ToCandle_GivenValuesOver2Periods_ShouldReturnTwoResults()
        {
            // arrange
            var dateTime = new DateTime(2001, 01, 01, 6, 0, 0);
            var historicalTrades = Builder<HistoricalTrade>.CreateListOfSize(3)
                .All()
                    .With(x=> dateTime = x.TradedAt = dateTime.AddSeconds(29))
                    .With(x => x.Price = 500)
                .TheFirst(1).With(x=>x.Price = 100)
                .TheLast(1).With(x => x.Price = 1)
                .Build();
            // action
            var candles = historicalTrades.ToCandleOneMinute().ToList();
            // assert
            candles.Should().HaveCount(2);
        }

        [Test]
        public void ToCandle_GivenTwoValues_ShouldSetOpenAndClose()
        {
            // arrange
            var dateTime = new DateTime(2001, 01, 01, 6, 0, 0);
            var historicalTrades = Builder<HistoricalTrade>.CreateListOfSize(3)
                .All()
                .With(x => dateTime = x.TradedAt = dateTime.AddSeconds(10))
                .TheFirst(1).With(x => x.Price = 100)
                .TheLast(1).With(x => x.Price = 50)
                .Build();
            // action
            var candles = historicalTrades.ToCandleOneMinute().ToList();
            // assert
            candles.Should().HaveCount(1);
            candles.First().Open.Should().Be(100);
            candles.First().Close.Should().Be(50);
        }

        [Test]
        public void ToCandle_GivenTwoValues_ShouldSetHighAndLow()
        {
            // arrange
            var dateTime = new DateTime(2001, 01, 01, 6, 0, 0);
            var historicalTrades = Builder<HistoricalTrade>.CreateListOfSize(3)
                .All()
                .With(x => dateTime = x.TradedAt = dateTime.AddSeconds(10))
                .TheFirst(1).With(x => x.Price = 20)
                .TheLast(2).With(x => x.Price = 50)
                .Build();
            // action
            var candles = historicalTrades.ToCandleOneMinute().ToList();
            // assert
            candles.Should().HaveCount(1);
            candles.First().High.Should().Be(50);
            candles.First().Low.Should().Be(20);
        }

        [Test]
        public void ToCandle_GivenTwoValues_ShouldSumVolume()
        {
            // arrange
            var dateTime = new DateTime(2001, 01, 01, 6, 0, 0);
            var historicalTrades = Builder<HistoricalTrade>.CreateListOfSize(2)
                .All()
                .With(x => dateTime = x.TradedAt = dateTime.AddSeconds(10))
                .TheFirst(1).With(x => x.Quantity = 20)
                .TheLast(1).With(x => x.Quantity = 50)
                .Build();
            // action
            var candles = historicalTrades.ToCandleOneMinute().ToList();
            // assert
            candles.Should().HaveCount(1);
            candles.First().Volume.Should().Be(70);
        }

        [Test]
        public void ToCandle_GivenGivenMissingValues_ShouldNotReturnAllQuotes()
        {
            // arrange
            var dateTime = new DateTime(2001, 01, 01, 6, 0, 0);
            var historicalTrades = Builder<HistoricalTrade>.CreateListOfSize(2)
                .All()
                .With(x => dateTime = x.TradedAt = dateTime.AddMinutes(5))
                .With(x => x.Price = 500)
                .TheFirst(1).With(x => x.Price = 100)
                .TheLast(1).With(x => x.Price = 1)
                .Build();
            // action
            var candles = historicalTrades.ToCandleOneMinute().ToList();
            // assert
            candles.Should().HaveCount(2);
            candles.First().Low.Should().Be(100);
            candles.Last().Low.Should().Be(1);
        }

    }
}
