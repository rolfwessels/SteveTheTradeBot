using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.Subscriptions;
using MediatR;

namespace SteveTheTradeBot.Core.Components.Users
{
    public class UserRealTimeEventHandler : RealTimeEventHandlerBase, INotificationHandler<UserCreate.Notification>,
        INotificationHandler<UserUpdate.Notification>,
        INotificationHandler<UserRemove.Notification>
    {
        private readonly SubscriptionNotifications _subscription;

        public UserRealTimeEventHandler(SubscriptionNotifications subscription)
        {
            _subscription = subscription;
        }

        #region Implementation of INotificationHandler<in Notification>

        public Task Handle(UserCreate.Notification notification, CancellationToken cancellationToken)
        {
            return _subscription.Send(BuildMessage(notification));
        }

        #endregion

        #region Implementation of INotificationHandler<in Notification>

        public Task Handle(UserUpdate.Notification notification, CancellationToken cancellationToken)
        {
            return _subscription.Send(BuildMessage(notification));
        }

        #endregion

        #region Implementation of INotificationHandler<in Notification>

        public Task Handle(UserRemove.Notification notification, CancellationToken cancellationToken)
        {
            return _subscription.Send(BuildMessage(notification));
        }

        #endregion
    }
}