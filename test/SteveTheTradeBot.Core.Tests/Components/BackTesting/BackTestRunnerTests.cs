using System.Threading;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.BackTesting
{

    public class BackTestRunnerTests
    {
        private BackTestRunner _backTestRunner;

        [Test]
        public async Task Run_GivenSmallAmountOfData_ShouldMakeNoTrades()
        {
            // arrange
            Setup();
            _backTestRunner = new BackTestRunner();
            var list =  Builder<HistoricalTrade>.CreateListOfSize(1000).WithValidData().Build().ToAsyncEnumerable();
            
            // action
            var backTestResult = await _backTestRunner.Run(list, new RSiBot(), CancellationToken.None);
            // assert
            backTestResult.TradesActive.Should().Be(1);
        }

        #region Setup/Teardown

        public void Setup()
        {

        }

        [TearDown]
        public void TearDown()
        {

        }

        #endregion
    }

    
}