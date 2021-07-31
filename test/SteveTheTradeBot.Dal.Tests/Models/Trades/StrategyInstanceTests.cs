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
            forBackTest.Reference.Should().StartWith("123_btczar_fiveminutes_202");
        }


        [Test]
        public void Get_GivenUnKnownProperty_ShouldDefault()
        {
            // action
            var forBackTest = StrategyInstance.ForBackTest("123", CurrencyPair.BTCZAR);
            // assert
            var result = forBackTest.Get("nothing", "default");
            result.Should().Be("default");
        }


        [Test]
        public void Get_GivenKnownProperty_ShouldValue()
        {
            // action
            var forBackTest = StrategyInstance.ForBackTest("123", CurrencyPair.BTCZAR);
            forBackTest.Set("nothing", "something");
            // assert
            var result = forBackTest.Get("nothing", "default");
            result.Should().Be("something");
        }

        [Test]
        public void Get_GivenNullProperty_ShouldNotFail()
        {
            // action
            var forBackTest = StrategyInstance.ForBackTest("123", CurrencyPair.BTCZAR);
            forBackTest.Property = null;
            // assert
            var result = forBackTest.Get("nothing", "default");
            result.Should().Be("default");
        }


        [Test]
        public void Set_GivenNullProperty_ShouldNotFail()
        {
            // action
            var forBackTest = StrategyInstance.ForBackTest("123", CurrencyPair.BTCZAR);
            forBackTest.Property = null;
            forBackTest.Set("nothing", "something");
            // assert
            var result = forBackTest.Get("nothing", "default");
            result.Should().Be("something");
        }

        [Test]
        public void Set_GivenExistingProperty_ShouldNotFail()
        {
            // action
            var forBackTest = StrategyInstance.ForBackTest("123", CurrencyPair.BTCZAR);
            forBackTest.Property = null;
            forBackTest.Set("nothing", "something1");
            forBackTest.Set("nothing", "something2");
            // assert
            var result = forBackTest.Get("nothing", "default");
            result.Should().Be("something2");
        }
    }
}