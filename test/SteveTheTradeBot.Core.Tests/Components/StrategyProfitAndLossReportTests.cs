using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components
{
    public class StrategyProfitAndLossReportTests
    {
        private ITradePersistenceFactory _factory;
        private StrategyProfitAndLossReport _report;

        [Test]
        public async Task Run_GivenNoData_ShouldReturn()
        {
            // arrange
            Setup();
            // action
            var records = await _report.Run();
            // assert
            records.Should().HaveCount(0);
        }


        [Test]
        public async Task Run_GivenSomeData_ShouldReturn()
        {
            // arrange
            Setup();
            var instances = Builder<StrategyInstance>.CreateListOfSize(5).WithValidData().Build();
            
            var context = await _factory.GetTradePersistence();
            context.Strategies.AddRange(instances);
            context.SaveChanges();

            // action
            var records = await _report.Run();
            // assert
            Console.Out.WriteLine(records.ToTable());
            records.Should().HaveCount(5);
        }


        private void Setup()
        {
            _factory = TestTradePersistenceFactory.UniqueDb();
            
            _report = new StrategyProfitAndLossReport(_factory);
        }
    }
}