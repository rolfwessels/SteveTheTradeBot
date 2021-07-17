using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public interface IUpdateHistoricalData
    {
        Task StartUpdate(string currencyPair);
    }
}