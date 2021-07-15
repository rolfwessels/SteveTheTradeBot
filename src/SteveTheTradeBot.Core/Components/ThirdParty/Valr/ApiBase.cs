using System;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using RateLimiter;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class ApiBase
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        protected RestClient _client;
        protected static TimeLimiter _rateLimit = TimeLimiter.GetFromMaxCountByInterval(30, TimeSpan.FromSeconds(60));

        public ApiBase(string baseUrl)
        {
            _client = new RestClient(baseUrl);
            _client.UseNewtonsoftJson();
        }

        public virtual T ValidateResponse<T>(IRestResponse<T> result)
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
}