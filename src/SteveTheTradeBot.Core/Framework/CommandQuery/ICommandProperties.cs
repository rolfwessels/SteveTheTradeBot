using System;

namespace SteveTheTradeBot.Core.Framework.CommandQuery
{
    public interface ICommandProperties
    {
        string CorrelationId { get; set; }
        string Id { get; set; }
        DateTime CreatedAt { get; set; }
    }
}