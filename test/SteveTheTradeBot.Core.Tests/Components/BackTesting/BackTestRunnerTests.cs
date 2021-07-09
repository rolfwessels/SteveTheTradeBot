using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;

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
            
            // action
            await _backTestRunner.Run(DateTime.Now.AddMonths(10), DateTime.Now, new RSiBot());
            // assert
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