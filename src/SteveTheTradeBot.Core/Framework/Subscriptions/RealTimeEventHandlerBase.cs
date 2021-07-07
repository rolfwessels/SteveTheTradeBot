using SteveTheTradeBot.Core.Framework.CommandQuery;

namespace SteveTheTradeBot.Core.Framework.Subscriptions
{
    public class RealTimeEventHandlerBase
    {
        protected RealTimeNotificationsMessage BuildMessage(CommandNotificationBase notification)
        {
            return new RealTimeNotificationsMessage()
            {
                CorrelationId = notification.CorrelationId,
                Event = notification.EventName,
                Id = notification.Id
            };
        }
    }
}