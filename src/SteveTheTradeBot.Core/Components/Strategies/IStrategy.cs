using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public interface IStrategy
    {
        string Name { get; }
        Task DataReceived(StrategyContext data);
        Task SellAll(StrategyContext strategyContext);
    }
}