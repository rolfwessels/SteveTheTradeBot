using System.Linq;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core.Tests.Utils
{
    public class EnumerableHelperTests
    {
        [Test]
        public void BatchedBy_GivenValues_ShouldBatchValues()
        {
            // arrange
            var ints = Enumerable.Range(0,33);
            // action
            var list = ints.BatchedBy(10).ToList();
            // assert
            list.Count.Should().Be(4);
        }


        [Test]
        public void BatchedBy_GivenValues_ShouldHaveEqualBatches()
        {
            // arrange
            var ints = Enumerable.Range(0, 20);
            // action
            var list = ints.BatchedBy(10).ToList();
            // assert
            list[0].Count.Should().Be(10);
            list[1].Count.Should().Be(10);
        }


        [Test]
        public void BatchedBy_GivenValues_ShouldReturnAllValues()
        {
            // arrange
            var ints = Enumerable.Range(0, 5);
            // action
            var list = ints.BatchedBy(2).ToList();
            // assert
            list.SelectMany(x=>x).Should().BeEquivalentTo(ints);
            list.SelectMany(x => x).Count().Should().Be(5);
        }

    }
}