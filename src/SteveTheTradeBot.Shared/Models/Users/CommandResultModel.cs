using System;

namespace SteveTheTradeBot.Shared.Models.Users
{
    public class CommandResultModel
    {
        public string CorrelationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Id { get; set; }
    }
}