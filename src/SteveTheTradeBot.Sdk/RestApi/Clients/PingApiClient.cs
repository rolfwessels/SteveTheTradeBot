using System.Threading.Tasks;
using SteveTheTradeBot.Sdk.Helpers;
using SteveTheTradeBot.Sdk.RestApi.Base;
using SteveTheTradeBot.Shared;
using SteveTheTradeBot.Shared.Models.Ping;
using RestSharp;

namespace SteveTheTradeBot.Sdk.RestApi.Clients
{
    public class PingApiClient : BaseApiClient
    {
        public PingApiClient(SteveTheTradeBotClient steveTheTradeBotClient) : base(steveTheTradeBotClient, RouteHelper.PingController)
        {
        }

        public async Task<PingModel> Get()
        {
            var restRequest = new RestRequest(DefaultUrl());
            var executeAsyncWithLogging = await SteveTheTradeBotClient.Client.ExecuteAsyncWithLogging<PingModel>(restRequest);
            return ValidateResponse(executeAsyncWithLogging);
        }
    }
}