using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class BotTrades : BaseDalModelWithId
    {
        public string BotInstanceId { get; set; }
        
    }
}