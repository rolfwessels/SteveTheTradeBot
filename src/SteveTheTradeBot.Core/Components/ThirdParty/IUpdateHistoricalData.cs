using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Components.ThirdParty
{
    public interface IUpdateHistoricalData
    {
        Task StartUpdate(string currencyPair);
    }
}