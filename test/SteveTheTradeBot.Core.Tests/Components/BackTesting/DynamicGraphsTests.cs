using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Tests.Components.Storage;

namespace SteveTheTradeBot.Core.Tests.Components.BackTesting
{
    public class DynamicGraphsTests
    {
        [Test]
        public async Task Plot_WhenCalledShouldSavePlotValuesToDb()
        {
            // arrange
            var dynamicGraphs = new DynamicGraphs(TestTradePersistenceFactory.InMemoryDb);
            // action
            for (int i = -5; i < 0; i++)
            {
                await dynamicGraphs.Plot("feed", DateTime.Now.AddMinutes(i), "test", i);
            }

            await dynamicGraphs.Flush();
            // assert
            var tradePersistenceStoreContext = await TestTradePersistenceFactory.InMemoryDb.GetTradePersistence();
            var list = tradePersistenceStoreContext.DynamicPlots.AsQueryable().Where(x=>x.Feed == "feed").ToList();
            list.Should().HaveCount(5);
        }

        [Test]
        public async Task Clear_WhenShouldClearDb()
        {
            // arrange
            var dynamicGraphs = new DynamicGraphs(TestTradePersistenceFactory.InMemoryDb);
            await dynamicGraphs.Plot("feed", DateTime.Now.AddMinutes(1), "test", 1);
            await dynamicGraphs.Flush();
            // action
            await dynamicGraphs.Clear("feed");
            // assert
            var tradePersistenceStoreContext = await TestTradePersistenceFactory.InMemoryDb.GetTradePersistence();
            var list = tradePersistenceStoreContext.DynamicPlots.AsQueryable().Where(x => x.Feed == "feed").ToList();
            list.Should().HaveCount(0);
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
