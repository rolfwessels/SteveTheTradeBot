namespace SteveTheTradeBot.Core.Components.Broker.Models
{
    public enum Side
    {
        Sell,Buy
    }
    
    public enum TimeEnforce
    {
        GoodTillCancelled, 
        FillOrKill,
        ImmediateOrCancel
    }

    public class CurrencyPair
    {
        public const string BTCZAR = "BTCZAR";
        public const string ETHZAR = "ETHZAR";
        public const string XRPZAR = "XRPZAR";
    }
}
