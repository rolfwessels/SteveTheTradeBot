using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Utils
{
    public class ProfitAndLossCalculatorTests
    {
        private ProfitAndLossCalculator _profitAndLossCalculator;

        [Test]
        public void GetDailyProfitAndLosses_GivenNoTransactions_ShouldReturnNoValues()
        {
            // arrange
            Setup();
            var strategyInstance = StrategyInstance.From("123", CurrencyPair.ETHZAR, 123, PeriodSize.OneMinute);
            // action
            var monthlyProfitAndLosses = _profitAndLossCalculator.GetDailyProfitAndLosses(strategyInstance);
            // assert
            monthlyProfitAndLosses.Should().BeEmpty();
        }

        [Test]
        public void GetDailyProfitAndLosses_GivenOneTransactionInOneDay_ShouldReturnSingleValue()
        {
            // arrange
            Setup();
            var strategyInstance = StrategyInstance.From("123", CurrencyPair.ETHZAR, 123, PeriodSize.OneMinute);
            strategyInstance.AddTrade(new DateTime(2001, 01, 01, 05, 00, 00), 1000, 0.5m);
            SellCurrentTrade(strategyInstance, new DateTime(2001, 01, 01, 06, 00, 00), 1010);
            // action
            var monthlyProfitAndLosses = _profitAndLossCalculator.GetDailyProfitAndLosses(strategyInstance);
            // assert
            monthlyProfitAndLosses.Should().HaveCount(1);
            monthlyProfitAndLosses.FirstOrDefault().Date.Should().Be(new DateTime(2001, 01, 01));
        }


        [Test]
        public void GetDailyProfitAndLosses_GivenOneTransactionInOneDay_ShouldReturnProfitForTheDay()
        {
            // arrange
            Setup();
            var strategyInstance = StrategyInstance.From("123", CurrencyPair.ETHZAR, 123, PeriodSize.OneMinute);
            strategyInstance.AddTrade(new DateTime(2001, 01, 01, 05, 00, 00), 1000, 0.5m);
            SellCurrentTrade(strategyInstance, new DateTime(2001, 01, 01, 06, 00, 00), 1010);
            // action
            var monthlyProfitAndLosses = _profitAndLossCalculator.GetDailyProfitAndLosses(strategyInstance);
            // assert
            monthlyProfitAndLosses.Should().HaveCount(1);
            monthlyProfitAndLosses.FirstOrDefault().Profit.Should().Be(1);
        }
     [Test]
        public void GetDailyProfitAndLosses_GivenOneTransactionInOneDay_ShouldReturnTheReturnOfTheDay()
        {
            // arrange
            Setup();
            var strategyInstance = StrategyInstance.From("123", CurrencyPair.ETHZAR, 123, PeriodSize.OneMinute);
            strategyInstance.AddTrade(new DateTime(2001, 01, 01, 05, 00, 00), 1000, 0.5m);
            SellCurrentTrade(strategyInstance, new DateTime(2001, 01, 01, 06, 00, 00), 1010);
            // action
            var monthlyProfitAndLosses = _profitAndLossCalculator.GetDailyProfitAndLosses(strategyInstance);
            // assert
            monthlyProfitAndLosses.Should().HaveCount(1);
            monthlyProfitAndLosses.FirstOrDefault().Return.Should().Be(1.01m);
        }

       

        private void SellCurrentTrade(StrategyInstance strategyInstance, DateTime dateTime, int tradeSellPrice)
        {
            var trade = strategyInstance.Trades.Last(x=>x.IsActive);
            trade.EndDate = dateTime;
            trade.SellValue = tradeSellPrice* trade.BuyQuantity;
            trade.SellPrice = tradeSellPrice;
            trade.Profit =  TradeUtils.MovementPercent(trade.SellValue, trade.BuyValue);
            trade.IsActive = false;
            trade.FeeAmount = 0;
        }

        private void Setup()
        {
            _profitAndLossCalculator = new ProfitAndLossCalculator();
        }
    }
}