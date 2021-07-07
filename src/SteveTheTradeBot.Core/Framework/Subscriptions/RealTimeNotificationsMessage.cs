namespace SteveTheTradeBot.Core.Framework.Subscriptions
{
    public class RealTimeNotificationsMessage
    {
        public string Id { get; set; }
        public string Event { get; set; }
        public string CorrelationId { get; set; }
        public string Exception { get; set; }
    }
}