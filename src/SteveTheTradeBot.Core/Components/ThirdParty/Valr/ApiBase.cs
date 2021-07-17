using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Bumbershoot.Utilities.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
        private JsonSerializerSettings _options;

        public ApiBase(string baseUrl)
        {
            _client = new RestClient(baseUrl);

            _options = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.None
            };
       
            _client.UseNewtonsoftJson(_options);
        }

        protected string? GetBody(RestRequest request)
        {
            return request.Parameters.Where(x => x.ContentType == "application/json")
                .Select(x => JsonConvert.SerializeObject(x.Value, _options))
                .FirstOrDefault();
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