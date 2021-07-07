using System;

namespace SteveTheTradeBot.Core.Framework.CommandQuery
{
    public class CommandResult : ICommandProperties
    {
        public string CorrelationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Id { get; set; }
    }
}