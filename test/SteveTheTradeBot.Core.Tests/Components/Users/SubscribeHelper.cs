using System;
using System.Collections.Generic;
using System.Linq;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Core.Framework.Subscriptions;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using MediatR;

namespace SteveTheTradeBot.Core.Tests.Components.Users
{
    public class SubscribeHelper
    {
        public static void NotificationScanner(Type type1, params string[] excludeNotifications)
        {
            var type = type1;
            var allNotifications = type.Assembly.Types()
                .Where(x => x.Namespace == type.Namespace)
                .Where(x => typeof(CommandNotificationBase).IsAssignableFrom(x));
            // action
            var list = new List<Type>();
            foreach (var notification in allNotifications)
            {
                var notificationHandler = typeof(INotificationHandler<>).MakeGenericType(notification);
                if (!type.GetInterfaces().Contains(notificationHandler) &&
                    !excludeNotifications.Contains(notification.Name))
                    list.Add(notification);
            }

            // assert
            var dictionary = list.ToDictionary(x => x.FullName.Split(".").Last().Replace("+", "."));
            dictionary.Keys.Dump($"Missing: [{list.Count}]");
            dictionary.Select(notification => $"{type.Name} should implement INotificationHandler<{notification.Key}>.")
                .ToArray().Should().BeEmpty();
        }

        public static void BasicNotificationValidation(RealTimeNotificationsMessage realTimeNotificationsMessage,
            CommandNotificationBase notification, string @event)
        {
            realTimeNotificationsMessage.CorrelationId.Should().Be(notification.CorrelationId);
            realTimeNotificationsMessage.Id.Should().Be(notification.Id);
            realTimeNotificationsMessage.Event.Should().Be(@event);
        }
    }
}