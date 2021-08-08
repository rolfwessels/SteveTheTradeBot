using System.Collections.Generic;
using System.Linq;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public class ResponseBuilder : IResponseBuilder
    {
        private readonly List<IResponder> _responders;

        public ResponseBuilder(IEnumerable<IResponder> messenger)
        {
            _responders = messenger.ToList();
            _responders.Add(new HelpResponder(_responders));
        }

        public  List<IResponder> GetResponders()
        {
            return _responders;
        }
    }
}