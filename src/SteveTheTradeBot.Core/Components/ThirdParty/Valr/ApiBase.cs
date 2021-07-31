using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class ApiBase
    {
        
        protected RestClient _client;
        
        private readonly JsonSerializerSettings _options;
        private readonly HttpStatusCode[] _validStatusCodes = new [] {HttpStatusCode.OK,HttpStatusCode.Accepted,HttpStatusCode.Created};

        public ApiBase(string baseUrl)
        {
            _client = new RestClient(baseUrl);

            _options = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Converters = new List<JsonConverter>() {
                    new Newtonsoft.Json.Converters.StringEnumConverter()
                },
                Formatting = Formatting.None
            };
       
            _client.UseNewtonsoftJson(_options);
        }

        protected string GetBody(RestRequest request)
        {
            return request.Parameters.Where(x => x.ContentType == "application/json")
                .Select(x => JsonConvert.SerializeObject(x.Value, _options))
                .FirstOrDefault();
        }
        

        public virtual T ValidateResponse<T>(IRestResponse<T> result)
        {
            if (!_validStatusCodes.Contains(result.StatusCode))
            {
                if (string.IsNullOrEmpty(result.Content))
                    throw new ApplicationException(
                        $"{_client.BuildUri(result.Request)} {result.StatusCode} response contains no data.");
                var errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(result.Content);
                throw new ApiResponseException(errorMessage.Message, result, result.StatusCode);
            }
            return result.Data;
        }
    }

    public class ApiResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public IRestResponse Response { get; }

        public ApiResponseException(string message, IRestResponse response, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
            Response = response;
        }
    }
}