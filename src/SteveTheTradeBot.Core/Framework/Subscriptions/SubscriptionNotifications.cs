using System;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.MessageUtil;

namespace SteveTheTradeBot.Core.Framework.Subscriptions
{
    public class SubscriptionNotifications
    {
        private readonly IMessenger _redisMessenger;

        public SubscriptionNotifications(IMessenger redisMessenger)
        {
            _redisMessenger = redisMessenger;
        }

        #region Implementation of IRealTimeNotificationChannel

        public Task Send(RealTimeNotificationsMessage message)
        {
            return _redisMessenger.Send(message);
        }

        #endregion

        public IDisposable Register(Action<RealTimeNotificationsMessage> action)
        {
            _redisMessenger.Register(this,action);
            return new Closer(() => _redisMessenger.UnRegister<RealTimeNotificationsMessage>(this));

        }

        private class Closer : IDisposable
        {
            private readonly Action _unRegister;

            public Closer(Action unRegister)
            {
                _unRegister = unRegister;
            }

            #region IDisposable

            public void Dispose()
            {
                _unRegister();
            }

            #endregion
        }
    }

}