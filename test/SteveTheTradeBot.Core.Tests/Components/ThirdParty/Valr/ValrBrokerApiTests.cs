using System;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Tests.Components.BackTesting;
using SteveTheTradeBot.Core.Tests.Components.Bots;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Components.ThirdParty.Valr
{
    [Category("Integration")]
    public class ValrBrokerApiTests
    {
        private ValrBrokerApi _api;


        [Test]
        public async Task SimpleOrder_GivenFakeBroker_ShouldGetFeeAndAmountsCorrect()
        {
            var fakeBroker = new FakeBroker {AskPrice = 461786, BuyFeePercent = 0.0075m};
            // action
            var response = await fakeBroker.Order(SimpleOrderRequest.From(Side.Buy, 100m, "ZAR", DateTime.Now, Gu.Id(), CurrencyPair.BTCZAR));
            // assert
            response.PaidAmount.Should().Be(100);
            response.PaidCurrency.Should().Be("ZAR");
            response.FeeCurrency.Should().Be("BTC");
            response.ReceivedCurrency.Should().Be("BTC");
            response.ReceivedAmount.Should().BeInRange(0.00021492m, 0.00021493m);
            response.FeeAmount.Should().Be(0.000001624125m);
            response.OrderExecutedAt.Should().BeCloseTo(DateTime.Now,2000);
            response.OrderId.Should().HaveLength(39);
        }

        [Test]
        public async Task SimpleOrder_GivenFakeBrokerAndSellOrder_ShouldGetFeeAndAmountsCorrect()
        {
            var fakeBroker = new FakeBroker { BidPrice = 460001, BuyFeePercent = 0.0075m };
            // action
            var response = await fakeBroker.Order(SimpleOrderRequest.From(Side.Sell, 0.0001m, "BTC", DateTime.Now, Gu.Id(), CurrencyPair.BTCZAR));
            // assert
            response.PaidAmount.Should().Be(0.0001m);
            response.PaidCurrency.Should().Be("BTC");
            response.FeeCurrency.Should().Be("ZAR");
            response.ReceivedCurrency.Should().Be("ZAR");
            response.ReceivedAmount.Should().BeInRange(45.65m, 45.66m);
            response.FeeAmount.Should().Be(0.34m);
            response.OrderExecutedAt.Should().BeCloseTo(DateTime.Now,2000);
            response.OrderId.Should().HaveLength(39);
        }



        [Test]
        public async Task OrderBook_GivenRequest_ShouldExecute()
        {
            // arrange
            Setup();
            await TestHelper.TestEveryNowAndThen(async () =>
            {
                if (!ValrSettings.Instance.ApiKey.StartsWith("ENC"))
                {
                    ValrSettings.Instance.Secret.Should().NotStartWith("ENC");
                    // action
                    var result = await _api.OrderBook(CurrencyPair.BTCZAR);
                    // assert
                    result.Asks.Count.Should().BeGreaterThan(40);
                }
            });
        }

        [Test]
        public async Task Quote_GivenRequest_ShouldRequestQuote()
        {
            // arrange
            Setup();
            await TestHelper.TestEveryNowAndThen(async () =>
            {
                if (!ValrSettings.Instance.ApiKey.StartsWith("ENC"))
                {
                    var valrHistoricalDataApi = new ValrHistoricalDataApi();

                    ValrSettings.Instance.Secret.Should().NotStartWith("ENC");
                    // action
                    var marketSummary = await valrHistoricalDataApi.GetMarketSummary(CurrencyPair.BTCZAR);
                    var result = await _api.Quote(SimpleOrderRequest.From(Side.Sell, 0.0001m, "BTC", DateTime.Now, Gu.Id(),
                        CurrencyPair.BTCZAR));

                    marketSummary.Dump("Mart");
                    result.Dump("result");
                    // assert
                    result.CurrencyPair.Should().Be(CurrencyPair.BTCZAR);
                    result.Fee.Should().BeGreaterThan(0);
                    result.ReceiveAmount.Should().BeGreaterThan(0);
                    result.FeeCurrency.Should().Be("ZAR");
                }
            });
        }

        [Test]
        public void SignBody_GivenSetValues_ShouldSignCorrectly()
        {
            // arrange
            Setup();
            _api = new ValrBrokerApi(ValrSettings.Instance.ApiKey,
                "4961b74efac86b25cce8fbe4c9811c4c7a787b7a5996660afcc2e287ad864363");
            // action
            var signBody = _api.SignBody("1558014486185", "GET", "/v1/account/balances");
            // assert
            signBody.Should()
                .Be(
                    "9d52c181ed69460b49307b7891f04658e938b21181173844b5018b2fe783a6d4c62b8e67a03de4d099e7437ebfabe12c56233b73c6a0cc0f7ae87e05f6289928");
        }

        [Test]
        public void SignBody_GivenSetValuesAndBody_ShouldSignCorrectly()
        {
            // arrange
            Setup();
            _api = new ValrBrokerApi(ValrSettings.Instance.ApiKey,
                "4961b74efac86b25cce8fbe4c9811c4c7a787b7a5996660afcc2e287ad864363");
            // action
            var signBody = _api.SignBody("1558017528946", "POST", "/v1/orders/market",
                @"{""customerOrderId"":""ORDER-000001"",""pair"":""BTCZAR"",""side"":""BUY"",""quoteAmount"":""80000""}");
            // assert
            signBody.Should()
                .Be(
                    "be97d4cd9077a9eea7c4e199ddcfd87408cb638f2ec2f7f74dd44aef70a49fdc49960fd5de9b8b2845dc4a38b4fc7e56ef08f042a3c78a3af9aed23ca80822e8");
        }

        private void Setup()
        {
            TestLoggingHelper.EnsureExists();
            _api = new ValrBrokerApi(ValrSettings.Instance.ApiKey, ValrSettings.Instance.Secret);
        }
    }
}