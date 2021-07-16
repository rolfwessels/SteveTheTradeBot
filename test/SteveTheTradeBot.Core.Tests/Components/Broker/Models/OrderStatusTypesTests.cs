using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker.Models;

namespace SteveTheTradeBot.Core.Tests.Components.Broker.Models
{
    public class OrderStatusTypesTests
    {

        [TestCase("Filled",OrderStatusTypes.Filled)]
        [TestCase("filled",OrderStatusTypes.Filled)]
        [TestCase("placed", OrderStatusTypes.Placed)]
        [TestCase("partially filled", OrderStatusTypes.PartiallyFilled)]
        [TestCase("failed", OrderStatusTypes.Failed)]
        public void ToOrderStatus_GivenString_ShouldSetEnum(string status, OrderStatusTypes expected)
        {
            var orderStatusTypes = OrderStatusTypesHelper.ToOrderStatus(status);
            orderStatusTypes.Should().Be(expected);
        }
    }
}