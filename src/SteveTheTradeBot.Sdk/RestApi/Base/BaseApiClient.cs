using System;
using System.Net;
using System.Threading.Tasks;
using SteveTheTradeBot.Sdk.Helpers;
using SteveTheTradeBot.Shared;
using SteveTheTradeBot.Shared.Models.Shared;
using Newtonsoft.Json;
using RestSharp;

namespace SteveTheTradeBot.Sdk.RestApi.Base
{
    public abstract class BaseApiClient
    {
        private readonly string _baseUrl;
        protected SteveTheTradeBotClient SteveTheTradeBotClient;

        protected BaseApiClient(SteveTheTradeBotClient steveTheTradeBotClient, string baseUrl)
        {
            SteveTheTradeBotClient = steveTheTradeBotClient;
            _baseUrl = baseUrl;
        }

        protected virtual string DefaultUrl(string appendToUrl = null)
        {
            return SteveTheTradeBotClient.UrlBase.AppendUrl(_baseUrl).AppendUrl(appendToUrl);
        }

        protected virtual string DefaultTokenUrl(string appendToUrl = null)
        {
            return SteveTheTradeBotClient.UrlBase.AppendUrl(_baseUrl).AppendUrl(appendToUrl);
        }

        protected virtual T ValidateResponse<T>(IRestResponse<T> result)
        {
            if (result.StatusCode != HttpStatusCode.OK)
            {
                if (string.IsNullOrEmpty(result.Content))
                    throw new ApplicationException(
                        $"{result.StatusCode} response contains no data.");
                var errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(result.Content);
                throw new Exception(errorMessage.Message);
            }

            return result.Data;
        }

        protected async Task<T> ExecuteAndValidate<T>(RestRequest request) where T : new()
        {
            var response = await SteveTheTradeBotClient.Client.ExecuteAsyncWithLogging<T>(request);
            ValidateResponse(response);
            return response.Data;
        }

        protected async Task<bool> ExecuteAndValidateBool(RestRequest request)
        {
            var response = await SteveTheTradeBotClient.Client.ExecuteAsyncWithLogging<bool>(request);
            ValidateResponse(response);
            return Convert.ToBoolean(response.Content);
        }
    }
}