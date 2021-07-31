using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.ThirdParty.Valr
{
    public class ValrBrokerPaperTradingApiTests
    {
        private ValrBrokerPaperTradingApi _valrBrokerApi;


        [Test]
        [Category("FullIntegration")]
        public async Task Order_GivenBuyOrder1_ShouldFulfillOrder()
        {
            await TestHelper.TestEveryNowAndThen(async () => {
                // arrange
                Setup();
                // action
                var response = await _valrBrokerApi.MarketOrder(SimpleOrderRequest.From(Side.Buy, 100, CurrencyCodes.ZAR,
                    DateTime.Now, Gu.Id(),
                    CurrencyPair.ETHZAR));
                // assert

                response.OrderStatusType.Should().Be("Filled");
                response.CurrencyPair.Should().Be("ETHZAR");
                response.AveragePrice.Should().BeGreaterOrEqualTo(35000);
                response.OriginalPrice.Should().Be(100);
                response.RemainingQuantity.Should().Be(0);
                response.OriginalQuantity.Should().BeApproximately(0.002m,0.1m);
                response.Total.Should().Be(100);
                response.TotalFee.Should().BeApproximately(0.00002m, 0.1m);
                response.FeeCurrency.Should().Be("ETH");
                response.OrderSide.Should().Be(0);
                response.OrderType.Should().Be("simple");
                response.FailedReason.Should().Be(null);
               
            });
        }


        private void Setup()
        {
            TestLoggingHelper.EnsureExists();
            _valrBrokerApi = new ValrBrokerPaperTradingApi(ValrSettings.Instance.ApiKey, ValrSettings.Instance.Secret, Messenger.Default);
        }
    }
}