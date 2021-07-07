using System;
using MediatR;

namespace SteveTheTradeBot.Core.Framework.CommandQuery
{
    public class CommandRequestBase : IRequest<CommandResult>, ICommandProperties
    {
        public CommandRequestBase()
        {
            CorrelationId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now.ToUniversalTime();
        }

        public override string ToString()
        {
            return $"{GetType().Name}_{CorrelationId}";
        }

        #region Implementation of ICommandProperties

        public string CorrelationId { get; set; }
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }

        #endregion
    }
}