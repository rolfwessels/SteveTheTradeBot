using System;
using System.Linq;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Tests.Components.BackTesting;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;
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
            var fakeBroker = new FakeBroker(Messenger.Default) {AskPrice = 461786, BuyFeePercent = 0.0075m};
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
            var fakeBroker = new FakeBroker(Messenger.Default) { BidPrice = 460001, BuyFeePercent = 0.0075m };
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
        public async Task BuildLimitOrderRequest_GivenSimpleOrderRequest_ShouldBuildRequestWithSameValuesAsQuotes()
        {
            Setup();
            await TestHelper.TestEveryNowAndThen(async () =>
            {
                if (!ValrSettings.Instance.ApiKey.StartsWith("ENC"))
                {
                    ValrSettings.Instance.Secret.Should().NotStartWith("ENC");
                    // action
                    var simpleOrderRequest = SimpleOrderRequest.From(Side.Buy, 10000m, "ZAR", DateTime.Now, Gu.Id(), CurrencyPair.BTCZAR);
                    var response = await _api.BuildLimitOrderRequest(simpleOrderRequest);
                    var quote = await _api.Quote(simpleOrderRequest);
                    // assert

                    response.Side.Should().Be(Side.Buy);
                    // todo: Rolf This needs more work but we are going to use the market order
                    response.Quantity.Should().BeApproximately(quote.ReceiveAmount + quote.Fee,0.0003m);
                    response.Price.Should().BeApproximately(quote.PayAmount/(quote.ReceiveAmount+quote.Fee),1000m);
                    response.Pair.Should().Be(simpleOrderRequest.CurrencyPair);
                    response.CustomerOrderId.Should().Be(simpleOrderRequest.CustomerOrderId);
                    response.CustomerOrderId.Should().Be(simpleOrderRequest.CustomerOrderId);
                    response.DateTime.Should().Be(simpleOrderRequest.RequestDate);
                    response.PostOnly.Should().Be(true);
                    response.TimeInForce.Should().Be(TimeEnforce.FillOrKill);
                }
            });
        }


        [Test]
        [Explicit]
        public async Task StopLimitOrder_GivenStopLimitOrderRequest_ShouldPlaceStopLimit()
        {
            Setup();
            if (!ValrSettings.Instance.ApiKey.StartsWith("ENC"))
            {
                ValrSettings.Instance.Secret.Should().NotStartWith("ENC");
                // action
                var task = await _api.Quote(SimpleOrderRequest.From(Side.Sell, 0.0001m, "BTC", DateTime.Now, Gu.Id(), CurrencyPair.BTCZAR));
                var marketPrice = task.OrdersToMatch.Average(x => x.Price);
                var price = marketPrice * 0.99m;
                var stopPrice = price * 1.001m;
                var customerOrderId = Gu.Id();
                var stopLimitOrderRequest = new StopLimitOrderRequest(Side.Sell, 0.0001m, price, CurrencyPair.BTCZAR, customerOrderId, TimeEnforce.FillOrKill, stopPrice, StopLimitOrderRequest.Types.StopLossLimit);
                stopLimitOrderRequest.Dump("stopLimitOrderRequest");
                // assert
                var response = await _api.StopLimitOrder(stopLimitOrderRequest);
                var status = await _api.OrderStatusById(CurrencyPair.BTCZAR, response.Id);
                // assert
                response.Id.Should().NotBeEmpty();
                status.OrderStatusType.Should().Be("Active");
            }
        }

        [Test]
        [Explicit]
        public async Task Cancel_GivenStopLimitOrderId_ShouldCancelStopOrder()
        {
            Setup();
            if (!ValrSettings.Instance.ApiKey.StartsWith("ENC"))
            {
                ValrSettings.Instance.Secret.Should().NotStartWith("ENC");
                // action
                await _api.CancelOrder("5e91e0d9-c362-4fc3-9c68-854adf25d3b4", CurrencyPair.BTCZAR);
                // assert
            }
        }


        [Test]
        public void ToMarketOrder_GivenSimpleOrderRequestToSell_ShouldBuildMarketOrder()
        {
            Setup();
            var simpleOrderRequest = SimpleOrderRequest.From(Side.Sell, 0.0001m, "BTC", DateTime.Now, Gu.Id(),
                CurrencyPair.BTCZAR);
            var response = _api.ToMarketOrder(simpleOrderRequest);
            // assert

            response.Side.Should().Be(Side.Sell);
            response.BaseAmount.Should().Be(0.0001m);
            response.QuoteAmount.Should().Be(null);
            response.Pair.Should().Be(simpleOrderRequest.CurrencyPair);
            response.CustomerOrderId.Should().Be(simpleOrderRequest.CustomerOrderId);
            response.CustomerOrderId.Should().Be(simpleOrderRequest.CustomerOrderId);
            response.DateTime.Should().Be(simpleOrderRequest.RequestDate);
        }

        [Test]
        public void ToMarketOrder_GivenSimpleOrderRequest_ShouldBuildMarketOrder()
        {
            Setup();
            var simpleOrderRequest = SimpleOrderRequest.From(Side.Buy, 10000m, "ZAR", DateTime.Now, Gu.Id(), CurrencyPair.BTCZAR);
            var response = _api.ToMarketOrder(simpleOrderRequest);
            // assert

            response.Side.Should().Be(Side.Buy);
            response.BaseAmount.Should().Be(null);
            response.QuoteAmount.Should().Be(10000m);
            response.Pair.Should().Be(simpleOrderRequest.CurrencyPair);
            response.CustomerOrderId.Should().Be(simpleOrderRequest.CustomerOrderId);
            response.CustomerOrderId.Should().Be(simpleOrderRequest.CustomerOrderId);
            response.DateTime.Should().Be(simpleOrderRequest.RequestDate);
        }

        [Test]
        public async Task OrderHistory_GivenGivenRequest_ShouldReturnPrevOrders()
        {
            Setup();
            await TestHelper.TestEveryNowAndThen(async () =>
            {
                var orderId = "32d798c4-e4a0-4794-9615-02c0d3df3d98";
                var status = await _api.OrderStatusByOrderId(CurrencyPair.BTCZAR, orderId);
                var summary = await _api.OrderHistorySummaryById(orderId);
                var response = await _api.OrderHistory();
                // assert
                status.Dump("status");
                summary.Dump("summary");
                response.Dump("Orders").Should().HaveCountGreaterThan(0);
            });
        }

        [Test]
        [Explicit]
        public async Task MarketOrder_GivenSimpleOrderBuyRequest_ShouldTakeAllTheStepsToMakeAMarketOrder()
        {
            Setup();
            if (!ValrSettings.Instance.ApiKey.StartsWith("ENC"))
            {
                ValrSettings.Instance.Secret.Should().NotStartWith("ENC");
                var simpleOrderRequest = SimpleOrderRequest.From(Side.Buy, 11m, "ZAR", DateTime.Now, Gu.Id(), CurrencyPair.BTCZAR);
                simpleOrderRequest.CustomerOrderId = "bfa0315f28a44106bd7b3fa65e235420";
                // action

                var response = await _api.MarketOrder(simpleOrderRequest);
                // assert
                
                response.FeeCurrency.Should().Be("BTC");
                response.OrderId.Should().HaveLength(36);
                response.AveragePrice.Should().Be(595472);
                response.OriginalPrice.Should().Be(20M);
                response.OriginalQuantity.Should().BeApproximately(0.00003486m, 0.00001m);
                response.Total.Should().Be(19.99594976M);
                response.TotalFee.Should().BeApproximately(0.00000003358m, 0.000000001m);
                response.OrderUpdatedAt.Should().BeCloseTo(DateTime.Parse("2021-07-28 06:45:00.88"), 5000);
            }

        }

        [Test]
        [Explicit]
        public async Task MarketOrder_GivenSimpleOrderSellRequest_ShouldTakeAllTheStepsToMakeAMarketOrder()
        {
            Setup();
            if (!ValrSettings.Instance.ApiKey.StartsWith("ENC"))
            {
                ValrSettings.Instance.Secret.Should().NotStartWith("ENC");
                var simpleOrderRequest = SimpleOrderRequest.From(Side.Sell, 0.00010254m, "BTC", DateTime.Now, Gu.Id(), CurrencyPair.BTCZAR);
                simpleOrderRequest.CustomerOrderId = "2541daf60b474aeb90d9eab2a1453bce";
                // action
                var response = await _api.MarketOrder(simpleOrderRequest);
                // assert
                
                response.FeeCurrency.Should().Be("ZAR");
                response.OrderId.Should().HaveLength(36);
                response.OriginalQuantity.Should().Be(0.00010254m);
                response.Total.Should().BeApproximately(61.40987298m, 0.00001m);
                response.TotalFee.Should().BeApproximately(0.06140987298m, 0.000000001m);
                response.OrderUpdatedAt.Should().BeCloseTo(DateTime.Parse("2021-07-28 07:21:43.156"), 5000);

            }

            // summary:
            // {
            //     "OrderId": "32d798c4-e4a0-4794-9615-02c0d3df3d98",
            //     "CustomerOrderId": "cf84700acee24b27b5f329f92cf49553",
            //     "OrderStatusType": "Filled",
            //     "CurrencyPair": "BTCZAR",
            //     "AveragePrice": 609196,
            //     "OriginalPrice": 608974,
            //     "RemainingQuantity": 0,
            //     "OriginalQuantity": 0.0001,
            //     "Total": 60.9196,
            //     "TotalFee": 0.0609196,
            //     "FeeCurrency": "ZAR",
            //     "OrderSide": 0,
            //     "OrderType": "stop-loss-limit",
            //     "FailedReason": "",
            //     "OrderUpdatedAt": "2021-07-28T12:42:13.796Z",
            //     "OrderCreatedAt": "2021-07-28T11:50:53.012Z"
            // }

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
        public async Task OrderHistorySummary_GivenForValidStopLoss_ShouldThatGotActivated()
        {
            // arrange
            Setup();
            await TestHelper.TestEveryNowAndThen(async () =>
            {
                if (!ValrSettings.Instance.ApiKey.StartsWith("ENC"))
                {
                    ValrSettings.Instance.Secret.Should().NotStartWith("ENC");
                    // action
                    var response = await _api.OrderStatusById(CurrencyPair.BTCZAR, "f2e9623d-f5df-4140-ac08-aa2bacf1042d");
                    // assert
                    response.OrderId.Should().HaveLength(36);
                    response.OrderStatusType.Should().Be("Cancelled");
                }
            });
        }

        [Test]
        public async Task OrderHistorySummary_GivenForStopLoss_ShouldCancelled()
        {
            // arrange
            Setup();
            await TestHelper.TestEveryNowAndThen(async () =>
            {
                if (!ValrSettings.Instance.ApiKey.StartsWith("ENC"))
                {
                    ValrSettings.Instance.Secret.Should().NotStartWith("ENC");
                    // action
                    var response = await _api.OrderStatusById(CurrencyPair.BTCZAR,"3ddf533f-0b73-4aaf-8d87-7f5af85aefdc");
                    // assert
                    response.OrderId.Should().HaveLength(36);
                    response.OrderStatusType.Should().Be("Cancelled");
                }
            });
        }

        [Test]
        public async Task OrderHistorySummary_GivenRequest_ShouldExecute()
        {
            // arrange
            Setup();
            await TestHelper.TestEveryNowAndThen(async () =>
            {
                if (!ValrSettings.Instance.ApiKey.StartsWith("ENC"))
                {
                    ValrSettings.Instance.Secret.Should().NotStartWith("ENC");
                    // action
                    var response = await _api.OrderHistorySummary("bfa0315f28a44106bd7b3fa65e235420");
                    // assert
                    response.FeeCurrency.Should().Be("BTC");
                    response.OrderId.Should().HaveLength(36);
                    response.OriginalPrice.Should().Be(20);
                    response.Total.Should().Be(19.99594976M);
                    response.OriginalQuantity.Should().Be(0.00003358M);
                    response.TotalFee.Should().BeApproximately(0.00000003358M, 0.000000001m);
                    response.OrderCreatedAt.Should().BeCloseTo(DateTime.Parse("2021-07-28 06:45:00.881"), 5000);
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