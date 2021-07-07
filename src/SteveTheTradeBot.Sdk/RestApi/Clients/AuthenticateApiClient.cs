using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using SteveTheTradeBot.Sdk.Helpers;
using SteveTheTradeBot.Sdk.RestApi.Base;
using SteveTheTradeBot.Shared.Models.Auth;
using RestSharp;

namespace SteveTheTradeBot.Sdk.RestApi.Clients
{
    public class AuthenticateApiClient : BaseApiClient
    {
        private readonly SteveTheTradeBotClient _steveTheTradeBotClient;

        public AuthenticateApiClient(SteveTheTradeBotClient steveTheTradeBotClient) : base(steveTheTradeBotClient, "connect")
        {
            _steveTheTradeBotClient = steveTheTradeBotClient;
        }

        public async Task<Jwks> GetConfigAsync()
        {
            var restRequest = new RestRequest(".well-known/openid-configuration/jwks");
            var restRequestAsyncHandle = await _steveTheTradeBotClient.Client.ExecuteAsyncWithLogging<Jwks>(restRequest);
            return restRequestAsyncHandle.Data;
        }

        public async Task<TokenResponseModel> Login(string adminUser, string adminPassword)
        {
            var token = await GetToken(new TokenRequestModel
            {
                ClientId = "stevethetradebot.api",
                ClientSecret = "super_secure_password",
                UserName = adminUser,
                Password = adminPassword
            });
            SteveTheTradeBotClient.SetToken(token);
            return token;
        }

        public async Task<TokenResponseModel> GetToken(TokenRequestModel tokenRequestModel)
        {
            var request = new RestRequest(DefaultTokenUrl("token"), Method.POST);
            request.AddParameter("client_id", tokenRequestModel.ClientId);
            request.AddParameter("client_secret", tokenRequestModel.ClientSecret);
            request.AddParameter("username", tokenRequestModel.UserName);
            request.AddParameter("password", tokenRequestModel.Password);
            request.AddParameter("grant_type", tokenRequestModel.GrantType);
            request.AddParameter("scope", "api");
            var restClient = _steveTheTradeBotClient.Client;
            var result =
                await restClient.ExecuteAsyncWithLogging<TokenResponseModel>(request);
            ValidateTokenResponse(result);
            return result.Data;
        }

        protected virtual void ValidateTokenResponse<T>(IRestResponse<T> result)
        {
            if (result.StatusCode != HttpStatusCode.OK)
            {
                if (string.IsNullOrEmpty(result.Content))
                    throw new ApplicationException(
                        $"{result.StatusCode} response contains no data.");
                var errorMessage = SimpleJson.DeserializeObject<TokenErrorMessage>(result.Content);
                throw new Exception($"{errorMessage.error}[{errorMessage.error_description}]");
            }
        }

        #region Nested type: Jwks

        public class Jwks
        {
            public List<Dictionary<string, string>> Keys { get; set; }
        }

        #endregion

        #region Nested type: TokenErrorMessage

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        internal class TokenErrorMessage
        {
            public string error { get; set; }
            public string error_description { get; set; }
        }

        #endregion
    }
}