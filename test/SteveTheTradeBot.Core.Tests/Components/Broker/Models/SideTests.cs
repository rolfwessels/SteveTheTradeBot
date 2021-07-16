using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker.Models;

namespace SteveTheTradeBot.Core.Tests.Components.Broker.Models
{
    public class SideTests
    {
        [Test]
        public void SideOut_GivenCurrencyPair_ShouldReturnTheMoneyOutCurrencyOnBuy()
        {
            // action
            var result = CurrencyPair.BTCZAR.SideOut(Side.Buy);
            // assert
            result.Should().Be("ZAR");
        }

        [Test]
        public void SideOut_GivenCurrencyPair_ShouldReturnTheMoneyOutCurrencyOnSell()
        {
            // action
            var result = CurrencyPair.BTCZAR.SideOut(Side.Sell);
            // assert
            result.Should().Be("BTC");
        }
    }
}