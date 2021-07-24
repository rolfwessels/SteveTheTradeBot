using System.Collections.Generic;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public class ResponseBuilder
    {
        public static List<IResponder> GetResponders()
        {
            var responders = new List<IResponder> { 
                new HelpResponder(new IResponder[0])
            };
            
            
            return responders;
        }
    }
}