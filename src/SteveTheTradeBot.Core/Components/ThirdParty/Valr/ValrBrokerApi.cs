using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using RestSharp;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class ValrBrokerApi : ApiBase, IBrokerApi
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _apiKey;
        private readonly string _secret;

        public ValrBrokerApi(string apiKey, string secret) : base("https://api.valr.com/v1/")
        {
            _apiKey = apiKey;
            _secret = secret;
        }

        #region Implementation of IBrokerApi

        public async Task<OrderBookResponse> OrderBook(string currencyPair)
        {
            _log.Information($"OrderBook {currencyPair}");
            var request = new RestRequest("marketdata/{currencyPair}/orderbook", DataFormat.Json) {Method = Method.GET};
            request.AddUrlSegment("currencyPair", currencyPair);
            return await ExecuteAsync<OrderBookResponse>(request);
        }

        public async Task<OrderHistorySummaryResponse> OrderHistorySummary(string customerOrderId)
        {
            _log.Information($"OrderHistorySummary customerOrderId {customerOrderId}");
            var request = new RestRequest("orders/history/summary/customerorderid/{customerOrderId}", DataFormat.Json) {Method = Method.GET};
            request.AddUrlSegment("customerOrderId", customerOrderId);
            return await ExecuteAsync<OrderHistorySummaryResponse>(request);
        }

        public async Task<IdResponse> SimpleOrder(SimpleOrderRequest simpleOrderRequest)
        {
            _log.Information($"SimpleOrder {simpleOrderRequest.CurrencyPair} {simpleOrderRequest.PayAmount} {simpleOrderRequest.PayInCurrency}");
            var request = new RestRequest("/simple/{currencyPair}/order", DataFormat.Json) { Method = Method.POST };
            request.AddUrlSegment("currencyPair", simpleOrderRequest.CurrencyPair);
            request.AddJsonBody(simpleOrderRequest);
            return await ExecuteAsync<IdResponse>(request);
        }

        public async Task<SimpleOrderStatusResponse> SimpleOrderStatus(string currencyPair, string orderId)
        {
            _log.Information($"OrderBook {currencyPair}");
            var request = new RestRequest("/simple/{currencyPair}/order/{id}", DataFormat.Json) { Method = Method.GET };
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddUrlSegment("id", orderId);
            return await ExecuteAsync<SimpleOrderStatusResponse>(request);
        }

        public async Task<SimpleOrderStatusResponse> Order(SimpleOrderRequest simpleOrderRequest)
        {
            var simpleOrder = await SimpleOrder(simpleOrderRequest);
            return await SimpleOrderStatus(simpleOrderRequest.CurrencyPair, simpleOrder.Id);
        }

        public Task CancelOrder(string brokerOrderId)
        {
            throw new NotImplementedException();
        }

        public Task SyncOrderStatus(StrategyInstance instance, StrategyContext strategyContext)
        {
            throw new NotImplementedException();
        }

        public async Task<SimpleOrderStatusResponse> MarketOrder(SimpleOrderRequest request)
        {

            var limitOrderRequest = await MarketOrderInternal(ToMarketOrder(request));
            //var limitOrderRequest = new IdResponse {Id = "883b35dc-263f-4252-9222-41012ce7bfb8"};
            var orderHistorySummary = await OrderHistorySummary(request.CustomerOrderId);
            return new SimpleOrderStatusResponse()
            {
                OrderId = orderHistorySummary.OrderId,
                Success = orderHistorySummary.OrderStatusType == "Filled",
                Processing = orderHistorySummary.OrderStatusType != "Filled",
                PaidAmount = orderHistorySummary.OriginalPrice,
                PaidCurrency = request.CurrencyPair.QuoteCurrency(),
                ReceivedAmount = orderHistorySummary.OriginalQuantity,
                ReceivedCurrency = request.CurrencyPair.BaseCurrency(),
                FeeAmount = orderHistorySummary.TotalFee,
                FeeCurrency = orderHistorySummary.FeeCurrency,
                FailedReason = orderHistorySummary.FailedReason,
                OrderExecutedAt = orderHistorySummary.OrderCreatedAt,
            };
        }

        private async Task<IdResponse> MarketOrderInternal(MarketOrderRequest toMarketOrder)
        {
            
            var request = new RestRequest("/orders/market", DataFormat.Json) { Method = Method.POST };
            request.AddJsonBody(toMarketOrder);
            var quoteResponse = await ExecuteAsync<IdResponse>(request);
            return quoteResponse;
        }

        public MarketOrderRequest ToMarketOrder(SimpleOrderRequest request)
        {
            var isBase = request.CurrencyPair.BaseCurrency() == request.PayInCurrency;

            decimal? baseAmount = null;
            decimal? quoteAmount = null;
            if (isBase)
            {
                baseAmount = request.PayAmount;
            }
            else
            {
                quoteAmount = request.PayAmount;
            }
            return new MarketOrderRequest(request.Side, quoteAmount, baseAmount, request.CurrencyPair,request.CustomerOrderId,request.RequestDate);
        }

        public async Task<LimitOrderRequest> BuildLimitOrderRequest(SimpleOrderRequest request)
        {
            var orderBookResponse = await OrderBook(request.CurrencyPair);
            var remaining = request.PayAmount ;
           
            var placeOrderFor= orderBookResponse.Asks
                .OrderBy(x => x.Price)
                .Where(x => remaining  >= 0)
                .ForAll(x=> remaining = (remaining - x.Value))
                .ToList().Dump("Values");
            remaining.Dump("remaining");
            
            var price = placeOrderFor.Average(x => x.Price);
            var quantity = Math.Round(request.PayAmount / price,8);
            var limitOrderRequest = new LimitOrderRequest(request.Side,quantity, price,request.CurrencyPair, request.CustomerOrderId, request.RequestDate,true);
            return limitOrderRequest;
        }

        public async Task<QuoteResponse> Quote(SimpleOrderRequest simpleOrderRequest)
        {
            _log.Information($"SimpleOrderQuote {simpleOrderRequest.CurrencyPair} {simpleOrderRequest.PayAmount} {simpleOrderRequest.PayInCurrency}");
            var request = new RestRequest("/simple/{currencyPair}/quote", DataFormat.Json) {Method = Method.POST};
            request.AddUrlSegment("currencyPair", simpleOrderRequest.CurrencyPair);
            request.AddJsonBody(AsRequest(simpleOrderRequest));
            var quoteResponse = await ExecuteAsync<QuoteResponse>(request);
            HackFixCurrency(quoteResponse);
            return quoteResponse;
        }

        private static void HackFixCurrency(QuoteResponse quoteResponse)
        {
            if (quoteResponse.FeeCurrency == "R") quoteResponse.FeeCurrency = "ZAR";
        }

        private object AsRequest(SimpleOrderRequest simpleOrderRequest)
        {
            return new {
                Side = simpleOrderRequest.Side.ToString().ToUpper(),
                PayInCurrency = simpleOrderRequest.PayInCurrency,
                PayAmount = simpleOrderRequest.PayAmount
            };
        }

        public Task<OrderStatusResponse> LimitOrder(LimitOrderRequest request)
        {
            throw new System.NotImplementedException();
        }

        public async Task<OrderStatusResponse> OrderStatus(string currencyPair,string customerOrderId)
        {
            var request = new RestRequest("/orders/{currencyPair}/customerorderid/{customerOrderId}", DataFormat.Json) { Method = Method.GET };
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddUrlSegment("customerOrderId", customerOrderId);
            return await ExecuteAsync<OrderStatusResponse>(request);
        }

        public async Task<OrderStatusResponse> OrderStatusByOrderId(string currencyPair, string orderId)
        {
            var request = new RestRequest("/orders/{currencyPair}/orderid/{orderId}", DataFormat.Json) { Method = Method.GET };
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddUrlSegment("orderId", orderId);
            return await ExecuteAsync<OrderStatusResponse>(request);
        }



        public Task<OrderStatusResponse> MarketOrder(MarketOrderRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<IdResponse> StopLimitOrder(StopLimitOrderRequest request)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Private Methods


        private void AddAuth(RestRequest request)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            request.AddHeader("X-VALR-API-KEY", _apiKey);
            request.AddHeader("X-VALR-SIGNATURE", SignBody(timestamp, request.Method.ToString(), _client.BuildUri(request).PathAndQuery,GetBody(request)));
            request.AddHeader("X-VALR-TIMESTAMP", timestamp);
        }

        protected async Task<T> ExecuteAsync<T>(RestRequest request)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            IRestResponse<T> response = null;
            var url = _client.BuildUri(request).PathAndQuery;
            try
            {
                AddAuth(request);

                response = await _client.ExecuteAsync<T>(request);
                var validateResponse = ValidateResponse(response);
                stopwatch.Stop();
                _log.Information($"Valid request [{stopwatch.Elapsed.ToShort()}] to {request.Method}:{url}");
                _log.Debug($"Request {request.Method}:{url} `{GetBody(request)}`");
                if (response.RawBytes.Length > 0)
                    _log.Debug($"Response {request.Method}:{url} `{Encoding.UTF8.GetString(response.RawBytes)}`");
                return validateResponse;
            }
            catch (Exception e)
            {

                _log.Warning($"Failed request [{stopwatch.Elapsed.ToShort()}] to {request.Method}:{url} {e.Message}");
                _log.Debug($"Request {request.Method}:{url} {GetBody(request)}");
                if (response?.RawBytes.Length > 0)
                    _log.Debug($"Response {request.Method}:{url} `{Encoding.UTF8.GetString(response.RawBytes)}`");
                throw;
            }
        }

        public string SignBody(string timestamp, string verb, string path, string? body = null)
        {
            var payload = timestamp + verb.ToUpper() + path + body;
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return hash.ToHexString();
        }

        #endregion

       
    }
}