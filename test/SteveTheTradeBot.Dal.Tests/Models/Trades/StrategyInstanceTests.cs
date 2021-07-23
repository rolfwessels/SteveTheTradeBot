using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Dal.Tests.Models.Trades
{
    public class StrategyInstanceTests
    {
        [Test]
        public void ForBackTest_GivenCurrencyPair_ShouldSetCorrectBaseAmount()
        {
            // action
            var forBackTest = StrategyInstance.ForBackTest("123", CurrencyPair.BTCZAR);
            // assert
            forBackTest.BaseAmount.Should().Be(1000);
            forBackTest.BaseAmountCurrency.Should().Be(CurrencyCodes.ZAR);
        }

        [Test]
        public void ForBackTest_GivenCurrencyPair_ShouldSetCorrectQuoteAmount()
        {
            // action
            var forBackTest = StrategyInstance.ForBackTest("123", CurrencyPair.BTCZAR);
            // assert
            forBackTest.QuoteAmount.Should().Be(0);
            forBackTest.QuoteAmountCurrency.Should().Be(CurrencyCodes.BTC);
        }


        [Test]
        public void ForBackTest_GivenCurrencyPair_ShouldSetIsBackTestTrue()
        {
            // action
            var forBackTest = StrategyInstance.ForBackTest("123", CurrencyPair.BTCZAR);
            // assert
            forBackTest.IsBackTest.Should().BeTrue();
        }

        [Test]
        public void ForBackTest_GivenCurrencyPair_ShouldSetPair()
        {
            // action
            var forBackTest = StrategyInstance.ForBackTest("123", CurrencyPair.BTCZAR);
            // assert
            forBackTest.Pair.Should().Be(CurrencyPair.BTCZAR);
        }

        [Test]
        public void ForBackTest_GivenCurrencyPair_ShouldReference()
        {
            // action
            var forBackTest = StrategyInstance.ForBackTest("123", CurrencyPair.BTCZAR);
            // assert
            forBackTest.Reference.Should().Be("123_btczar_fiveminutes_20210723");
        }

       

    }
}