using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class HistoricalDataApi
    {
        private readonly RestClient _client;

        public HistoricalDataApi(string baseUrl = "https://api.valr.com/v1/public/")
        {
            _client = new RestClient(baseUrl);
            _client.UseNewtonsoftJson();

        }

        public async Task<TradeResponseDto[]> GetTradeHistory(string currencyPair,int skip = 0, int limit = 100)
        {
            var request = new RestRequest("{currencyPair}/trades", DataFormat.Json);
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddQueryParameter("skip", skip.ToString());
            request.AddQueryParameter("limit", limit.ToString());
            var response = await _client.ExecuteGetAsync<TradeResponseDto[]>(request);
            

            return ValidateResponse(response);
        }
        public async Task<TradeResponseDto[]> GetTradeHistory(string currencyPair,string beforeId, int limit = 100)
        {
            var request = new RestRequest("{currencyPair}/trades", DataFormat.Json);
            request.AddUrlSegment("currencyPair", currencyPair);
            request.AddQueryParameter("beforeId", beforeId);
            request.AddQueryParameter("limit", limit.ToString());
            var response = await _client.ExecuteGetAsync<TradeResponseDto[]>(request);
                

            return ValidateResponse(response);
        }

        protected virtual T ValidateResponse<T>(IRestResponse<T> result)
        {
            if (result.StatusCode != HttpStatusCode.OK)
            {
                if (string.IsNullOrEmpty(result.Content))
                    throw new ApplicationException(
                        $"{_client.BuildUri(result.Request)} {result.StatusCode} response contains no data.");
                var errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(result.Content);
                throw new Exception(errorMessage.Message);
            }

            return result.Data;
        }

    }

    public class ErrorMessage
    {
        public string Message { get; set; }
    }


    public class TradeResponseDto
    {
        public string Price { get; set; }
        public string Quantity { get; set; }
        public string CurrencyPair { get; set; }
        public DateTime TradedAt { get; set; }
        public string TakerSide { get; set; }
        public int SequenceId { get; set; }
        public string Id { get; set; }
        public string QuoteVolume { get; set; }
    }
}