using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Utils;

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


       

        public class Root
        {
            public string orderId { get; set; }
            public bool success { get; set; }
            public bool processing { get; set; }
            public string paidAmount { get; set; }
            public string paidCurrency { get; set; }
            public string receivedAmount { get; set; }
            public string receivedCurrency { get; set; }
            public string feeAmount { get; set; }
            public string feeCurrency { get; set; }
            public DateTime orderExecutedAt { get; set; }
        }

        public async Task<OrderHistorySummaryResponse> OrderHistorySummary(string customerOrderId)
        {
            _log.Information($"OrderHistorySummary customerOrderId {customerOrderId}");
            var request = new RestRequest("orders/history/summary/customerorderid/{customerOrderId}", DataFormat.Json) {Method = Method.GET};
            request.AddUrlSegment("currencyPair", customerOrderId);
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