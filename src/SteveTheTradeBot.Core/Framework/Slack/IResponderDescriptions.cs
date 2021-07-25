using System.Collections.Generic;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public interface IResponderDescriptions
    {
        IEnumerable<IResponderDescription> Descriptions { get; }
    }
}