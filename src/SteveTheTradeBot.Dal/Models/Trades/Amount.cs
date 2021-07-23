namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class Amount
    {
        public decimal Value { get; set; }
        public string Currency { get; set; }

        public static Amount From(decimal @decimal, string sideOut)
        {
            return new Amount() {Value = @decimal, Currency = sideOut};
        }
    }
}