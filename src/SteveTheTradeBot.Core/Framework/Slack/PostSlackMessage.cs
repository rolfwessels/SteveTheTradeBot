namespace SteveTheTradeBot.Core.Framework.Slack
{
    public class PostSlackMessage
    {
        public string Message { get; set; }

        public static PostSlackMessage From(string message)
        {
            return new PostSlackMessage() { Message =  message };
        }
    }
}