using System;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Utils
{
    public class TradeUtilsTests
    {

        [Test]
        public void ForDate_GivenValuesInThePast_ShouldReturnNull()
        {
            // arrange
            var tradeFeedQuotes = Builder<TradeQuote>.CreateListOfSize(1)
                .All().WithValidData()
                .WithValidData().Build()
                .ForEach(x=>x.Date = new DateTime(2001,01,01,1,1,0))
                .ForEach(x => x.PeriodSize = PeriodSize.FiveMinutes);
            // action
            var result = tradeFeedQuotes.ForDate(new DateTime(2001, 01, 02, 1, 1, 0));
            // assert
            result.Should().BeNull();
        }


        [Test]
        public void ForDate_GivenValuesInTheFuture_ShouldReturnNull()
        {
            // arrange
            var dateTime = new DateTime(2001, 01, 01, 1, 1, 0);
            var tradeFeedQuotes = Builder<TradeQuote>.CreateListOfSize(1)
                .All().WithValidData()
                .WithValidData().Build()
                .ForEach(x => x.Date = dateTime)
                .ForEach(x => x.PeriodSize = PeriodSize.FiveMinutes);
            // action
            var result = tradeFeedQuotes.ForDate(dateTime.Add(PeriodSize.FiveMinutes.ToTimeSpan()));
            // assert
            result.Should().BeNull();
        }


        [Test]
        public void ForDate_GivenValuesInDate_ShouldReturnValue()
        {
            // arrange
            var dateTime = new DateTime(2001, 01, 01, 1, 1, 0);
            var tradeFeedQuotes = Builder<TradeQuote>.CreateListOfSize(1)
                .All().WithValidData()
                .WithValidData().Build()
                .ForEach(x => x.Date = dateTime)
                .ForEach(x => x.PeriodSize = PeriodSize.FiveMinutes);
            // action
            var result = tradeFeedQuotes.ForDate(dateTime);
            // assert
            result.Should().NotBeNull();
        }

        [Test]
        public void ForDate_GivenValuesJustInDate_ShouldReturnValue()
        {
            // arrange
            var dateTime = new DateTime(2001, 01, 01, 1, 0, 0);
            var tradeFeedQuotes = Builder<TradeQuote>.CreateListOfSize(1)
                .All().WithValidData()
                .WithValidData().Build()
                .ForEach(x => x.Date = dateTime)
                .ForEach(x => x.PeriodSize = PeriodSize.FiveMinutes);
            // action
            var result = tradeFeedQuotes.ForDate(dateTime.Add(PeriodSize.FiveMinutes.ToTimeSpan()).AddSeconds(-1));
            // assert
            result.Should().NotBeNull();
        }





    }
}