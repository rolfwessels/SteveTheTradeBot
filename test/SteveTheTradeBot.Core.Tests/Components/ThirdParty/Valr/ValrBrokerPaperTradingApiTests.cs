using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.ThirdParty.Valr
{
    public class ValrBrokerPaperTradingApiTests
    {
        private ValrBrokerPaperTradingApi _valrBrokerApi;


        [Test]
        public async Task Order_GivenBuyOrder_ShouldFulfillOrder()
        {
            await TestHelper.TestEveryNowAndThen(async () => {
                // arrange
                Setup();
                // action
                var response = await _valrBrokerApi.Order(SimpleOrderRequest.From(Side.Buy, 100, CurrencyCodes.ZAR,
                    DateTime.Now, Gu.Id(),
                    CurrencyPair.ETHZAR));
                // assert
                response.OrderId.Should().NotBeNullOrEmpty();
                response.Success.Should().Be(true);
                response.Processing.Should().Be(false);
                response.PaidAmount.Should().Be(100);
                response.PaidCurrency.Should().Be(CurrencyCodes.ZAR);
                response.ReceivedAmount.Should().BeGreaterThan(0).And.BeLessThan(1);
                response.ReceivedCurrency.Should().Be(CurrencyCodes.ETH);
                response.FeeAmount.Should().BeGreaterThan(0);
                response.FeeCurrency.Should().Be(CurrencyCodes.ETH);
                response.OrderExecutedAt.Should().BeCloseTo(DateTime.Now.ToUniversalTime(), 5999);
            });
        }


        private void Setup()
        {
            TestLoggingHelper.EnsureExists();
            _valrBrokerApi = new ValrBrokerPaperTradingApi(ValrSettings.Instance.ApiKey, ValrSettings.Instance.Secret);
        }
    }
}