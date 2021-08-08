using System.Threading.Tasks;
using SlackConnector.Models;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public abstract class ResponderBase : IResponder
    {
        public virtual bool CanRespond(MessageContext context)
        {
            return context.IsForBot();
        }

        public abstract Task GetResponse(MessageContext context);

       
    }

    
}