using SteveTheTradeBot.Sdk.RestApi.Clients;
using SteveTheTradeBot.Shared.Models.Auth;

namespace SteveTheTradeBot.Sdk.RestApi
{
    public interface ISteveTheTradeBotClient
    {
        ProjectApiClient Projects { get; }
        UserApiClient Users { get; }
        AuthenticateApiClient Authenticate { get; }
        PingApiClient Ping { get; }
        void SetToken(TokenResponseModel data);
    }
}