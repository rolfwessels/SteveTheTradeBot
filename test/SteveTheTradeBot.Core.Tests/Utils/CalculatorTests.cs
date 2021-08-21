using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core.Tests.Utils
{
    public class CalculatorTests
    {
        [Test]
        public void SharpeRatioForYear_GivenSampleValues1_ShouldCalculateSharpRatio()
        {
            // arrange
            // action
            var result = Calculator.SharpeRatioForYear(new[] { 4m, 4m, 4m, 4m, 4m, 4m, 4m, 4m, 4m, 5m, }, 3m);
            // assert
            result.Should().Be(3.48m);
        }


        [Test]
        public void SharpeRatioForYear_GivenSampleValues_ShouldCalculateSharpRatio()
        {
            // arrange
            // action
            var result = Calculator.SharpeRatioForYear(new [] {1.64m, 5.85m, 9.22m, 3.51m, -0.88m, 1.07m, 13.03m, 9.4m, 10.49m, -5.08m}, 3m);
            // assert
            result.Should().Be(0.32m);
        }

        [Test]
        public void SharpeRatioForYear_GivenSampleValue1s_ShouldCalculateSharpRatio()
        {
            
            // action
            var result = Calculator.SharpeRatioForYear(new[] { 1.64m, 5.85m, 9.22m, 3.51m, -0.88m, 1.07m, 13.03m, 9.4m }, 3m);
            // assert
            result.Should().Be(0.49m);
        }
    }
}