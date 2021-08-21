using AutoMapper.Internal;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Dal.Models.Trades;

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