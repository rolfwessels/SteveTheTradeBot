using System;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Dal.Tests.Models.Trades
{
    public class StrategyTradeTests
    {
        [Test]
        public void ToString_GivenStrategyInstance_ShouldDisplayRelevantInformationOnBy()
        {
            // arrange
            var forBackTest = StrategyInstance.ForBackTest("test",CurrencyPair.BTCZAR);
            var strategyTrade = forBackTest.AddTrade(new DateTime(2001, 01, 02, 03, 04, 05, DateTimeKind.Utc), 10000, 0.002m);
            // action
            var value = strategyTrade.ToString(forBackTest);
            // assert
            value.Should().Be("Bought 0.002BTC at R10000 for R20.00 (Fee R0)");
        }


    }
}