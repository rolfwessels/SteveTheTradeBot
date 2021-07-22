using System.Threading;
using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public interface IUpdateHistoricalData
    {
        Task PopulateNewThenOld(string currencyPair, CancellationToken token);
        Task UpdateHistory(string currencyPair, CancellationToken token);
        Task PopulateNewData(string currencyPair, CancellationToken token);
    }
}