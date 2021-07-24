namespace SteveTheTradeBot.Core.Framework.Slack
{
    public interface IResponderDescription
    {
        string Command { get;  }
        string Description { get;  }
    }
}