using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.BackTesting;

namespace SteveTheTradeBot.Core.Tests.Components.BackTesting
{
    public class DynamicGraphsTests
    {
        [Test]
        public void Todo()
        {
            // arrange
            1.Should().Be(2);
            // action

            // assert
        }


        public class FakeGraph : IDynamicGraphs
        {
            public FakeGraph()
            {
            }

            #region Implementation of IDynamicGraphs

            public Task Clear(string feedName)
            {
                return Task.CompletedTask;
            }

            public Task Plot(string feedName, DateTime date, string label, decimal value)
            {
                return Task.CompletedTask;
            }

            public Task Flush()
            {
                return Task.CompletedTask;
            }

            #endregion
        }
    }
}