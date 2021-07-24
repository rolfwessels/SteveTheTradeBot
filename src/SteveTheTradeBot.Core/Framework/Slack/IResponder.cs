using SlackConnector.Models;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public interface IResponder
    {
        bool CanRespond(MessageContext context);
        BotMessage GetResponse(MessageContext context);
    }
}