using System.Threading.Tasks;
using SlackConnector;
using SlackConnector.Models;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public class MessageContext : IMessageContext
    {
        private readonly ISlackConnection _connection;

        public MessageContext(SlackMessage message, bool botHasResponded, ISlackConnection connection)
        {
            _connection = connection;
            Message = message;
            IsFromSlackbot = botHasResponded;
        }

        public SlackMessage Message { get; }
        public bool IsFromSlackbot { get; }

        public Task Say(string text)
        {
            return Say(new BotMessage() { Text = text});
        }

        public string Text => this.CleanMessage();

        public Task SayOutput(string text)
        {
            if (string.IsNullOrEmpty(text)) return Task.FromResult(true);
            return Say(new BotMessage() { Text = $"```{text}```"});
        }

        public Task SayError(string text)
        {
            if (string.IsNullOrEmpty(text)) return Task.FromResult(true);
            return Say(new BotMessage() { Text = $">>>`{text}`"});
        }

        public Task Say(BotMessage botMessage)
        {
            if (botMessage.ChatHub == null) botMessage.ChatHub = Message.ChatHub;
            if (string.IsNullOrEmpty(botMessage.Text)) return Task.FromResult(true);
            return _connection.Say(botMessage);
        }

        public Task SayCode(string text)
        {
            return SayOutput(text);
        }
    }
}