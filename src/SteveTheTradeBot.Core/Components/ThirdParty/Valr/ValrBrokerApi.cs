using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        private string _secret;

        public ValrBrokerApi(string apiKey, string secret) : base("https://api.valr.com/v1/public/")
        {
            
            _apiKey = apiKey;
            _secret = secret;
        }

        #region Implementation of IBrokerApi

        public async Task<OrderBookResponse> OrderBook(string currencyPair)
        {
            _log.Information($"OrderBook {currencyPair}");
            var request = new RestRequest("marketdata/{currencyPair}/orderbook", DataFormat.Json);
            AddAuth(request);
            var response = await _client.ExecuteGetAsync<OrderBookResponse>(request);
            return ValidateResponse(response);
        }


        public Task<IdResponse> LimitOrder(LimitOrderRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<IdResponse> MarketOrder(MarketOrderRequest request)
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
            request.AddHeader("X-VALR-SIGNATURE", SignBody(timestamp, "Et", "asdf", ""));
            request.AddHeader("X-VALR-TIMESTAMP", timestamp);
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