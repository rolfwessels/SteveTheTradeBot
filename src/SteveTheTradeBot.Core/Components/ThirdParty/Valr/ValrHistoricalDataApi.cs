using System;
using System.Reflection;
using System.Threading.Tasks;
using ComposableAsync;
using Hangfire.Logging;
using RestSharp;
using Serilog;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class ValrHistoricalDataApi : ApiBase, IHistoricalDataApi
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public ValrHistoricalDataApi() : base("https://api.valr.com/v1/public/")
        {
           
        }


        public async Task<MarketSummaryResponse> GetMarketSummary(string currencyPair)
        {
            _log.Information($"GetMarketsummary {currencyPair} ");
            var request = new RestRequest("{currencyPair}/marketsummary", DataFormat.Json);
            request.AddUrlSegment("currencyPair", currencyPair);
            await _rateLimit;
            var response = await _client.ExecuteGetAsync<MarketSummaryResponse>(request);
            return ValidateResponse(response);
        }

       
        public async Task<TradeResponseDto[]> GetTradeHistory(string currencyPair,int skip = 0, int limit = 100)
        {
            _log.Information($"GetTradeHistory {currencyPair} skip={skip} limit={limit}");
            var request = new RestRequest("{currencyPair}/trades", DataFormat.Json);
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddQueryParameter("skip", skip.ToString());
            request.AddQueryParameter("limit", limit.ToString());
            await _rateLimit;
            var response = await _client.ExecuteGetAsync<TradeResponseDto[]>(request);
            return ValidateResponse(response);
        }

        public async Task<TradeResponseDto[]> GetTradeHistory(string currencyPair,string beforeId, int limit = 100)
        {
            _log.Information($"GetTradeHistory {currencyPair} beforeId={beforeId} limit={limit}");
            var request = new RestRequest("{currencyPair}/trades", DataFormat.Json);
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddQueryParameter("beforeId", beforeId);
            request.AddQueryParameter("limit", limit.ToString());
            await _rateLimit;
            var response = await _client.ExecuteGetAsync<TradeResponseDto[]>(request);
            return ValidateResponse(response);
        }

        public async Task<TradeResponseDto[]> GetTradeHistory(string currencyPair,DateTime startDateTime,DateTime endDateTime, int skip = 0, int limit = 100)
        {
            _log.Information($"GetTradeHistory {currencyPair} from {startDateTime.ToUniversalTime().ToIsoDateString()} to {endDateTime.ToUniversalTime().ToIsoDateString()}  skip={skip} limit={limit}");
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