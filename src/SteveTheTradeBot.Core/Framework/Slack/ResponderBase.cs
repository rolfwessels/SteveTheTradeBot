using SlackConnector.Models;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public abstract class ResponderBase : IResponder
    {
        public virtual bool CanRespond(MessageContext context)
        {
            return MessageContextHelper.IsForBot(context);
        }

        public abstract BotMessage GetResponse(MessageContext context);

       
    }

    
}