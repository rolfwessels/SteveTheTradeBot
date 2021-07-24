using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public interface IMessageContext
    {
        string Text { get; }
        Task SayOutput(string text);
        Task SayError(string text);
    }
}