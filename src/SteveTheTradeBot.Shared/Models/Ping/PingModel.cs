namespace SteveTheTradeBot.Shared.Models.Ping
{
    public class PingModel
    {
        public string Environment { get; set; }
        public string Version { get; set; }
        public string Database { get; set; }
        public string MachineName { get; set; }
    }
}