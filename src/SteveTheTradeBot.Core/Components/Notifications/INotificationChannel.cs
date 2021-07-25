using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Components.Notifications
{
    public interface INotificationChannel
    {
        Task PostAsync(string post);
        Task PostSuccessAsync(string message);
        Task PostFailedAsync(string message);
    }
}