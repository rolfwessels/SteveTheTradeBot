using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.BackTesting;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public interface IStrategy
    {
        Task DataReceived(BackTestRunner.BotData data);
        string Name { get; }
        Task SellAll(BackTestRunner.BotData botData);
    }
}