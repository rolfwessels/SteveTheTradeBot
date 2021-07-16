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

        

        public async Task<OrderHistorySummaryResponse> OrderHistorySummary(string customerOrderId)
        {
            _log.Information($"OrderHistorySummary customerOrderId {customerOrderId}");
            var request = new RestRequest("orders/history/summary/customerorderid/{customerOrderId}", DataFormat.Json) {Method = Method.GET};
            request.AddUrlSegment("currencyPair", customerOrderId);
            return await ExecuteAsync<OrderHistorySummaryResponse>(request);
        }
        
        public async Task<QuoteResponse> Quote(string currencyPair, Side side, decimal amount, string payIn)
        {
            _log.Information($"OrderBook {currencyPair}");
            var request = new RestRequest("/simple/{currencyPair}/quote", DataFormat.Json) {Method = Method.POST};
            request.AddUrlSegment("currencyPair", currencyPair);
            var quoteOrderRequest = new QuoteOrderRequest { Side = side.ToString().ToUpper() , PayAmount = amount.ToString(CultureInfo.InvariantCulture), PayInCurrency = payIn};
            request.AddJsonBody(quoteOrderRequest);
            return await ExecuteAsync<QuoteResponse>(request);
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
            var payload = timestamp + verb.ToUpper() + path + body.Dump("body");
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return hash.ToHexString();
        }

        #endregion


    }
}