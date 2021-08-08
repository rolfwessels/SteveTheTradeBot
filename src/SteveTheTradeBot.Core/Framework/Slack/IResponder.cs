using System.Threading.Tasks;
using SlackConnector.Models;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public interface IResponder
    {
        bool CanRespond(MessageContext context);
        Task GetResponse(MessageContext context);
    }
}