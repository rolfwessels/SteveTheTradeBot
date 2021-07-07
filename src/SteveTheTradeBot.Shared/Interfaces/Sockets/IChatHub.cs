using System.Threading.Tasks;

namespace SteveTheTradeBot.Shared.Interfaces.Sockets
{
    public interface IChatHub
    {
        Task Send(string message);
    }
}