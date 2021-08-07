using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using ComposableAsync;
using RateLimiter;
using RestSharp;
using Serilog;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class ValrHistoricalDataApi : ApiBase, IHistoricalDataApi
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        protected static TimeLimiter _rateLimit = TimeLimiter.GetFromMaxCountByInterval(40, TimeSpan.FromSeconds(60));
        public ValrHistoricalDataApi() : base("https://api.valr.com/v1/public/")
        {  
        }

        public async Task<MarketSummaryResponse> GetMarketSummary(string currencyPair)
        {
            var request = new RestRequest("{currencyPair}/marketsummary", DataFormat.Json) { Method = Method.GET };
            request.AddUrlSegment("currencyPair", currencyPair);
            await _rateLimit;
            return await Execute<MarketSummaryResponse>(request);
        }

        public async Task<TradeResponseDto[]> GetTradeHistory(string currencyPair,int skip = 0, int limit = 100)
        {
            var request = new RestRequest("{currencyPair}/trades", DataFormat.Json) { Method = Method.GET };
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddQueryParameter("skip", skip.ToString());
            request.AddQueryParameter("limit", limit.ToString());
            await _rateLimit;
            return await Execute<TradeResponseDto[]>(request);
        }

        public async Task<TradeResponseDto[]> GetTradeHistory(string currencyPair,string beforeId, int limit = 100)
        {
            var request = new RestRequest("{currencyPair}/trades", DataFormat.Json) { Method = Method.GET };
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddQueryParameter("beforeId", beforeId);
            request.AddQueryParameter("limit", limit.ToString());
            await _rateLimit;
            return await Execute<TradeResponseDto[]>(request);
        }

        private async Task<T> Execute<T>(RestRequest request)
        {
            var stopwatch = new Stopwatch();
            IRestResponse<T> response = null;
            try
            {
                stopwatch.Start();
                response = await _client.ExecuteGetAsync<T>(request);
                var validateResponse = ValidateResponse(response);
                stopwatch.Stop();
                _log.Debug($"Request to {_client.BuildUri(request).PathAndQuery} returned {response.StatusCode} in {stopwatch.Elapsed.ToShort()}");
                return validateResponse;
            }
            catch (Exception e)
            {
                _log.Debug($"[WRN] Request to {_client.BuildUri(request).PathAndQuery} failed with status `{response?.StatusCode}` in {stopwatch.Elapsed.ToShort()} : {e.Message}");
                throw;
            }
        }

        public async Task<TradeResponseDto[]> GetTradeHistory(string currencyPair,DateTime startDateTime,DateTime endDateTime, int skip = 0, int limit = 100)
        {
            var request = new RestRequest("{currencyPair}/trades", DataFormat.Json);
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddQueryParameter("startTime", startDateTime.ToUniversalTime().ToIsoDateString());
            request.AddQueryParameter("endTime", endDateTime.ToUniversalTime().ToIsoDateString());
            request.AddQueryParameter("skip", skip.ToString());
            request.AddQueryParameter("limit", limit.ToString());
            await _rateLimit;
            var response = await _client.ExecuteGetAsync<TradeResponseDto[]>(request);
            return ValidateResponse(response);
        }
    }
}