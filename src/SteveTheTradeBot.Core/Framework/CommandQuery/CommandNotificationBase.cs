using System;
using MediatR;

namespace SteveTheTradeBot.Core.Framework.CommandQuery
{
    public abstract class CommandNotificationBase : ICommandProperties, INotification
    {
        #region Implementation of ICommandProperties

        public string CorrelationId { get; set; }
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public abstract string EventName { get; }

        #endregion
    }
}