using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Utils
{
    public class StrategyInstanceHelperTests
    {

        [Test]
        public void Recalculate_GivenNewStrategy_ShouldHandleNoTrades()
        {
            // arrange
            var strategyInstance = StrategyInstance.From("123", CurrencyPair.ETHZAR, 123, PeriodSize.OneMinute);
            // action
            strategyInstance.Recalculate();
            // assert
            strategyInstance.TotalActiveTrades.Should().Be(0);
        }

        [Test]
        public void Recalculate_GivenOneTrade_ShouldBeAbleToCalculateMaxProfit()
        {
            // arrange
            var strategyInstance = StrategyInstance.From("123", CurrencyPair.ETHZAR, 123, PeriodSize.OneMinute);
            strategyInstance.Trades.Add(new StrategyTrade(DateTime.Now, 122,2,3));
            // action
            strategyInstance.Recalculate();
            // assert
            strategyInstance.TotalActiveTrades.Should().Be(1);
        }

        [Test]
        public void Recalculate_GivenTwoTradesInAMonth_ShouldBeAbleToCalculateMaxProfit()
        {
            // arrange
            var strategyInstance = StrategyInstance.From("123", CurrencyPair.ETHZAR, 123, PeriodSize.OneMinute);
            var strategyTrades = Builder<StrategyTrade>.CreateListOfSize(10).Build();
            strategyInstance.Trades.AddRange(strategyTrades);
            strategyInstance.FirstStart = DateTime.Now.AddDays(-60);
            strategyInstance.LastDate = DateTime.Now;
            // action
            strategyInstance.Recalculate();
            // assert
            strategyInstance.AverageTradesPerMonth.Should().Be(5);
        }

        [Test]
        public void Recalculate_GivenNoTrades_ShouldSetAverageTradesPerMonth()
        {
            // arrange
            var strategyInstance = StrategyInstance.From("123", CurrencyPair.ETHZAR, 123, PeriodSize.OneMinute);
            
            strategyInstance.FirstStart = DateTime.Now.AddDays(-60);
            strategyInstance.LastDate = DateTime.Now;
            // action
            strategyInstance.Recalculate();
            // assert
            strategyInstance.AverageTradesPerMonth.Should().Be(0);
        }
    }
}