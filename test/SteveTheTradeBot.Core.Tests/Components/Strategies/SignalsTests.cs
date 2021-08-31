using System;
using System.Linq;
using AutoMapper.Internal;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.Strategies
{
    public class SignalsTests
    {
        [Test]
        public void EmaIsUpTrend_GivenValidUpTrend_ShouldReturnTrue()
        {
            // arrange
            var tradeQuote = new TradeQuote();
            tradeQuote.Metric.Add(Signals.Ema200,100);
            tradeQuote.Close = 101;
            // action
            var isUpTrend = Signals.Ema.IsUpTrend(tradeQuote);
            // assert
            isUpTrend.Should().BeTrue();
        }

        [Test]
        public void EmaIsUpTrend_GivenCloseAtEma200_ShouldReturnTrue()
        {
            // arrange
            var tradeQuote = new TradeQuote();
            tradeQuote.Metric.Add(Signals.Ema200, 100);
            tradeQuote.Close = 101;
            // action
            var isUpTrend = Signals.Ema.IsUpTrend(tradeQuote);
            // assert
            isUpTrend.Should().BeTrue();
        }

        [Test]
        public void EmaIsUpTrend_GivenCloseBelowEma200_ShouldReturnFalse()
        {
            // arrange
            var tradeQuote = new TradeQuote();
            tradeQuote.Metric.Add(Signals.Ema200, 100);
            tradeQuote.Close = 99;
            // action
            var isUpTrend = Signals.Ema.IsUpTrend(tradeQuote);
            // assert
            isUpTrend.Should().BeFalse();
        }


        [Test]
        public void EmaIsPositiveTrend_GivenNoRangeChange_ShouldReturnFalse()
        {
            // arrange
            var fiveMinutes = PeriodSize.FiveMinutes;
            var minutes = fiveMinutes.ToTimeSpan().TotalMinutes;
            var size = 13;
            var tradeQuotes = Builder<TradeQuote>.CreateListOfSize(size)
                .WithValidData()
                .All()
                .With((x,i) =>  x.Date = DateTime.Now.ToMinute().AddMinutes(size-i * minutes) )
                .TheFirst(1)
                .With((x, i) => x.Metric.Add(Signals.Ema200, 100))
                .TheLast(1)
                .With((x, i) => x.Metric.Add(Signals.Ema200, 100))
                .Build();
            tradeQuotes.Select(x=>new { x.Date, Ema200= x.Metric.GetOrDefault(Signals.Ema200) }).PrintTable();
            
            // action
            var isUpTrend = Signals.Ema.IsPositiveTrend(tradeQuotes, fiveMinutes);
            // assert
            isUpTrend.Should().BeFalse();
        }


        [Test]
        public void EmaIsPositiveTrend_GivenPositiveRangeChange_ShouldReturnTrue()
        {
            // arrange
            var fiveMinutes = PeriodSize.FiveMinutes;
            var minutes = fiveMinutes.ToTimeSpan().TotalMinutes;
            var size = 13;
            var dateTime = new DateTime(2001,01,01,01,05,00,DateTimeKind.Utc);
            var tradeQuotes = Builder<TradeQuote>.CreateListOfSize(size)
                .WithValidData()
                .All()
                .With((x, i) =>
                {
                    
                    x.Date = dateTime.AddMinutes(-(size - i) * minutes);
                })
                .TheFirst(1)
                .With((x, i) => x.Metric.Add(Signals.Ema200, 99))
                .TheLast(1)
                .With((x, i) => x.Metric.Add(Signals.Ema200, 100))
                .Build();
            tradeQuotes.Select(x => new { x.Date, Ema200 = x.Metric.GetOrDefault(Signals.Ema200) }).PrintTable();

            // action
            var isUpTrend = Signals.Ema.IsPositiveTrend(tradeQuotes, fiveMinutes);
            // assert
            isUpTrend.Should().BeTrue();
        }


        [Test]
        public void EmaIsPositiveTrend_GivenNegativeRangeChange_ShouldReturnFalse()
        {
            // arrange
            var fiveMinutes = PeriodSize.FiveMinutes;
            var minutes = fiveMinutes.ToTimeSpan().TotalMinutes;
            var size = 13;
            var tradeQuotes = Builder<TradeQuote>.CreateListOfSize(size)
                .WithValidData()
                .All()
                .With((x, i) => x.Date = DateTime.Now.ToMinute().AddMinutes(size - i * minutes))
                .TheFirst(1)
                .With((x, i) => x.Metric.Add(Signals.Ema200, 100))
                .TheLast(1)
                .With((x, i) => x.Metric.Add(Signals.Ema200, 90))
                .Build();
            tradeQuotes.Select(x => new { x.Date, Ema200 = x.Metric.GetOrDefault(Signals.Ema200) }).PrintTable();

            // action
            var isUpTrend = Signals.Ema.IsPositiveTrend(tradeQuotes, fiveMinutes);
            // assert
            isUpTrend.Should().BeFalse();
        }

        [Test]
        public void MacdIsCrossedBelowZero_GivenCloseBelowEma200_ShouldReturnFalse()
        {
            // arrange
            var tradeQuote = new TradeQuote();
            tradeQuote.Metric.Add(Signals.MacdSignal, 1);
            tradeQuote.Metric.Add(Signals.MacdValue, 1);
            // action
            var isCrossedBelowZero = Signals.Macd.IsCrossedBelowZero(new [] { tradeQuote });
            // assert
            isCrossedBelowZero.Should().BeFalse();
        }

        [Test]
        public void MacdIsCrossedBelowZero_GivenMacdSignalValuesAtZero_ShouldReturnFalse()
        {
            // arrange
            var tradeQuote = new TradeQuote();
            tradeQuote.Metric.Add(Signals.MacdSignal, 0);
            tradeQuote.Metric.Add(Signals.MacdValue, 0);
            // action
            var isCrossedBelowZero = Signals.Macd.IsCrossedBelowZero(new[] { tradeQuote });
            // assert
            isCrossedBelowZero.Should().BeFalse();
        }

        [Test]
        public void MacdIsCrossedBelowZero_GivenMacdSignalValuesAtMinusOne_ShouldReturnTrue()
        {
            // arrange
            var tradeQuote = new TradeQuote();
            tradeQuote.Metric.Add(Signals.MacdSignal, -1);
            tradeQuote.Metric.Add(Signals.MacdValue, -1);
            // action
            var isCrossedBelowZero = Signals.Macd.IsCrossedBelowZero(new[] { tradeQuote });
            // assert
            isCrossedBelowZero.Should().BeTrue();
        }


        [Test]
        public void MacdIsCrossedBelowZero_GivenMacdSignalIsAbove0_ShouldReturnFalse()
        {
            // arrange
            var tradeQuote = new TradeQuote();
            tradeQuote.Metric.Add(Signals.MacdSignal, 2);
            tradeQuote.Metric.Add(Signals.MacdValue, -1);
            // action
            var isCrossedBelowZero = Signals.Macd.IsCrossedBelowZero(new[] { tradeQuote });
            // assert
            isCrossedBelowZero.Should().BeFalse();
        }


        [Test]
        public void MacdGetCrossed_GivenGivenOneValue_ShouldEmptyList()
        {
            // arrange
            var tradeQuotes = Builder<TradeQuote>.CreateListOfSize(1)
                .All()
                .With((x, i) => x.Metric.Add(Signals.MacdSignal, i))
                .With((x, i) => x.Metric.Add(Signals.MacdValue, i))
                .Build();

            // action
            var result = Signals.Macd.GetCrossedMacdOverSignal(tradeQuotes);
            // assert
            result.Should().HaveCount(0);
        }


        [Test]
        public void MacdGetCrossed_GivenGivenNoCrossOver_ShouldEmptyList()
        {
            // arrange
            var tradeQuotes = Builder<TradeQuote>.CreateListOfSize(6)
                .All()
                    .With((x,i)=> x.Metric.Add(Signals.MacdSignal, i))
                    .With((x, i) => x.Metric.Add(Signals.MacdValue, i))
                .Build();
            
            // action
            var result = Signals.Macd.GetCrossedMacdOverSignal(tradeQuotes);
            // assert
            result.Should().HaveCount(0);
        }


        [Test]
        public void MacdGetCrossed_GivenGivenOneCrossing_ShouldReturnInstance()
        {
            // arrange
            var tradeQuotes = Builder<TradeQuote>.CreateListOfSize(6)
                .All()
                .With((x, i) => x.Metric.Add(Signals.MacdValue,-3+i))
                .With((x, i) => x.Metric.Add(Signals.MacdSignal, 0))
                .Build().Dump("d");

            // action
            var result = Signals.Macd.GetCrossedMacdOverSignal(tradeQuotes);
            // assert
            result.Should().HaveCount(1);
            result[0].Metric.GetOrDefault(Signals.MacdValue).Should().Be(1);
            result[0].Metric.GetOrDefault(Signals.MacdSignal).Should().Be(0);
        }


        [Test]
        public void GetCrossedSignalOverMacd_GivenGivenOneValue_ShouldEmptyList()
        {
            // arrange
            var tradeQuotes = Builder<TradeQuote>.CreateListOfSize(1)
                .All()
                .With((x, i) => x.Metric.Add(Signals.MacdSignal, i))
                .With((x, i) => x.Metric.Add(Signals.MacdValue, i))
                .Build();

            // action
            var result = Signals.Macd.GetCrossedSignalOverMacd(tradeQuotes);
            // assert
            result.Should().HaveCount(0);
        }


        [Test]
        public void GetCrossedSignalOverMacd_GivenGivenOneCrossing_ShouldReturnInstance()
        {
            // arrange
            var tradeQuotes = Builder<TradeQuote>.CreateListOfSize(6)
                .All()
                .With((x, i) => x.Metric.Add(Signals.MacdValue, 3 - i))
                .With((x, i) => x.Metric.Add(Signals.MacdSignal, 0))
                .Build().Dump("d");

            // action
            var result = Signals.Macd.GetCrossedSignalOverMacd(tradeQuotes);
            // assert
            result.Should().HaveCount(1);
            result[0].Metric.GetOrDefault(Signals.MacdValue).Should().Be(-1);
            result[0].Metric.GetOrDefault(Signals.MacdSignal).Should().Be(0);
        }


    }
}