using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public interface IBot
    {
        Task DataReceived(BackTestRunner.BotData data);
        string Name { get; }
    }
}