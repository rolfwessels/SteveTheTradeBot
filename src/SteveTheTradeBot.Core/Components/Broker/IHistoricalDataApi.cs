using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public interface IHistoricalDataApi
    {
        Task<TradeResponseDto[]> GetTradeHistory(string currencyPair, int skip = 0, int limit = 100);
        Task<TradeResponseDto[]> GetTradeHistory(string currencyPair, string beforeId, int limit = 100);
    }
}