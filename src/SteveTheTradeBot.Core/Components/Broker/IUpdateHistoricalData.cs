using System.Threading;
using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public interface IUpdateHistoricalData
    {
        Task StartUpdate(string currencyPair, CancellationToken token);
        Task UpdateHistory(string currencyPair, CancellationToken token);
    }
}