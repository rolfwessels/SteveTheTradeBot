using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class SimpleParam : BaseDalModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}