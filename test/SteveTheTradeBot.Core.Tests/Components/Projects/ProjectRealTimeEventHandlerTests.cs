using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SteveTheTradeBot.Core.Components.Projects;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Framework.Subscriptions;
using SteveTheTradeBot.Core.Tests.Components.Users;
using SteveTheTradeBot.Core.Tests.Framework.BaseManagers;
using SteveTheTradeBot.Core.Tests.Helpers;
using SteveTheTradeBot.Dal.Tests;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests.Components.Projects
{
    [TestFixture]
    public class ProjectRealTimeEventHandlerTests : BaseManagerTests
    {
        private SubscriptionNotifications _subscriptionNotifications;
        private ProjectRealTimeEventHandler _projectRealTimeEventHandler;

        #region Overrides of BaseManagerTests

        public override void Setup()
        {
            base.Setup();
            _subscriptionNotifications = new SubscriptionNotifications(new Messenger());
            _projectRealTimeEventHandler = new ProjectRealTimeEventHandler(_subscriptionNotifications);
        }

        #endregion

        [Test]
        public void Scan_GivenProjectRealTimeEventHandler_ShouldNotBeMissingAnyNotifications()
        {
            Setup();
            SubscribeHelper.NotificationScanner(typeof(ProjectRealTimeEventHandler));
        }

        [Test]
        public void Handle_GivenProjectCreateNotification_ShouldNotifyOfProjectChange()
        {
            // arrange
            Setup();
            var notification = BuildNotification<ProjectCreate.Notification>();
            // action
            BasicTest(() => _projectRealTimeEventHandler.Handle(notification, CancellationToken.None), notification,
                "ProjectCreated", _subscriptionNotifications);
        }

        [Test]
        public void Handle_GivenProjectUpdateNotification_ShouldNotifyOfProjectChange()
        {
            // arrange
            Setup();
            var notification = BuildNotification<ProjectUpdateName.Notification>();
            // action
            BasicTest(() => _projectRealTimeEventHandler.Handle(notification, CancellationToken.None), notification,
                "ProjectUpdatedName", _subscriptionNotifications);
        }

        [Test]
        public void Handle_GivenProjectRemoveNotification_ShouldNotifyOfProjectChange()
        {
            // arrange
            Setup();
            var notification = BuildNotification<ProjectRemove.Notification>();
            // action
            BasicTest(() => _projectRealTimeEventHandler.Handle(notification, CancellationToken.None), notification,
                "ProjectRemoved", _subscriptionNotifications);
        }


        public void BasicTest(Action action, CommandNotificationBase notification,
            string @event, SubscriptionNotifications subscriptionNotifications)
        {
            var list = new List<RealTimeNotificationsMessage>();
            using (subscriptionNotifications.Register(message => list.Add(message)))
            {
                action();
                // assert
                list.WaitFor(x=>x.Count == 1);
                list.Count.Should().Be(1);
                SubscribeHelper.BasicNotificationValidation(list.First(), notification, @event);
            }
        }

        private static T BuildNotification<T>()
        {
            return Builder<T>.CreateNew().WithValidData().Build();
        }
    }
}