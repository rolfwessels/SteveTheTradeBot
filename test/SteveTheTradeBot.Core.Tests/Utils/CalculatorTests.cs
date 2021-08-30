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
        public void SharpeRatioForYear_GivenSampleValues3PercentRFR_ShouldCalculateSharpRatio()
        {
            // arrange
            // action
            var yearData = new [] {1.64m, 5.85m, 9.22m, 3.51m, -0.88m, 1.07m, 13.03m, 9.4m, 10.49m, -5.08m};
            var result = Calculator.SharpeRatioForYear(yearData, 3m);
            // assert
            result.Should().Be(0.32m);
        }

        [Test]
        public void SharpeRatioForYear_GivenSampleValueAt5PercentRFR_ShouldCalculateSharpRatio()
        {
            
            // action
            var result = Calculator.SharpeRatioForYear(new[] { 1.64m, 5.85m, 9.22m, 3.51m, -0.88m, 1.07m, 13.03m, 9.4m }, 5m);
            // assert
            result.Should().Be(0.07m);
        }


        [Test]
        public void SharpeRatioMonths_GivenSampleValues_ShouldCalculateSharpRatio()
        {

            // action
            var monthData = new[] { 1.64m, 5.85m, 9.22m, 3.51m, -0.88m, 1.07m, 13.03m, 9.4m, 10.49m, -5.08m };
            var result = Calculator.SharpeRatioOverPeriods(monthData, 12, 5m);
            // assert
            result.Should().Be(-0.111m);
        }

        
    }
}