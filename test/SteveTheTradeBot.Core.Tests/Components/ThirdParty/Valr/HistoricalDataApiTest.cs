using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.ThirdParty.Valr
{
    public class HistoricalDataApiTest
    {
        private HistoricalDataApi _historicalDataApi;

        #region Setup/Teardown

        public void Setup()
        {
            _historicalDataApi = new HistoricalDataApi();
        }

        #endregion

        [Test]
        public async Task GetTradeHistory_GivenBTCZAR_ShouldLimitResults()
        {
            await TestHelper.TestEveryNowAndThen(async () =>
            {
                // arrange
                Setup();
                // action
                var tradeResponses = await _historicalDataApi.GetTradeHistory("BTCZAR", limit: 10);
                // assert
                tradeResponses.Dump("d").Should().HaveCount(10);
            });
        }


        [Test]
        public async Task GetTradeHistory_GivendBTCZARAndAIdBefore_ShouldLimitResults()
        {
            await TestHelper.TestEveryNowAndThen(async () =>
            {
                // arrange
                Setup();
                // action
                var tradeResponses = await _historicalDataApi.GetTradeHistory("BTCZAR", "e4de58db-b09c-4688-9f8f-39c1c1aba6d7",11);
                // assert
                tradeResponses.Should().HaveCount(11);
            });
        }

    }
}