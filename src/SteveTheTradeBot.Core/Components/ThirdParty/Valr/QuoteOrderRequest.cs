namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public class QuoteOrderRequest
    {
        public string PayInCurrency { get; set; }
        public string PayAmount { get; set; }
        public string Side { get; set; }
    }
}