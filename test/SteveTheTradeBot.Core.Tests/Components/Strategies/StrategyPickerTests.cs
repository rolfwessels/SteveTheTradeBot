using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Strategies;

namespace SteveTheTradeBot.Core.Tests.Components.Strategies
{
    public class StrategyPickerTests
    {
        [Test]
        public void Add_GivenStrategy_ShouldAllowGet()
        {
            // arrange
            var strategyPicker = new StrategyPicker();
            strategyPicker.Add("123", () => new Mock<IStrategy>().Object);
            // action
            var x = strategyPicker.Get("123");
            // assert
            x.Should().NotBe(null);
        }

        [Test]
        public void Add_GivenInvalid_ShouldAllowGet()
        {
            // arrange
            var strategyPicker = new StrategyPicker();
            strategyPicker.Add("123", () => new Mock<IStrategy>().Object);
            // action
            Action testCall = () =>
            {
                strategyPicker.Get("1233");
            };
            // assert
            testCall.Should().Throw<ArgumentOutOfRangeException>();
        }


    }
}