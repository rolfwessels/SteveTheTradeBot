using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.ThirdParty.Valr
{
    public class ValrHistoricalDataApiTests
    {
        private ValrHistoricalDataApi _valrHistoricalDataApi;

        #region Setup/Teardown

        public void Setup()
        {
            _valrHistoricalDataApi = new ValrHistoricalDataApi();
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
                var tradeResponses = await _valrHistoricalDataApi.GetTradeHistory(CurrencyPair.BTCZAR, limit: 10);
                // assert
                tradeResponses.Should().HaveCount(10);
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
                var tradeResponses =
                    await _valrHistoricalDataApi.GetTradeHistory(CurrencyPair.BTCZAR,
                        "e4de58db-b09c-4688-9f8f-39c1c1aba6d7", 11);
                // assert
                tradeResponses.Should().HaveCount(11);
            });
        }


        [Test]
        public async Task GetTradeHistory_GivendBTCZARAndDates_ShouldReturnFromEndDateBackToStartDate()
        {
            await TestHelper.TestEveryNowAndThen(async () =>
            {
                // arrange
                Setup();
                // action
                var startDateTime = new DateTime(2021, 01, 01, 13, 00, 0, DateTimeKind.Utc);
                var endDateTime = startDateTime.AddHours(0.5);
                var tradeResponses =
                    await _valrHistoricalDataApi.GetTradeHistory(CurrencyPair.BTCZAR, startDateTime, endDateTime, 0,
                        11);
                // assert
                tradeResponses.Should().HaveCount(11);
                tradeResponses[0].TradedAt.Should().BeCloseTo(endDateTime, TimeSpan.FromMinutes(5));
                tradeResponses[10].TradedAt.Should().BeBefore(tradeResponses[0].TradedAt);
            });
        }
    }
}