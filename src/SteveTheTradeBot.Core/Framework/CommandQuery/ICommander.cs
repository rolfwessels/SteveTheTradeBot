using System.Threading;
using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Framework.CommandQuery
{
    public interface ICommander
    {
        Task Notify<T>(T notificationRequest, CancellationToken cancellationToken) where T : CommandNotificationBase;
        Task<CommandResult> Execute<T>(T commandRequest, CancellationToken cancellationToken) where T : CommandRequestBase;
    }
}