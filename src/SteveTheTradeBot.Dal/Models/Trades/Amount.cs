using System;

namespace SteveTheTradeBot.Dal.Models.Trades
{
    public class Amount
    {
        public decimal Value { get; set; }
        public string Currency { get; set; }

        public static Amount From(decimal value, string currency)
        {
            return new Amount() {Value = value, Currency = currency};
        }

        public override string ToString()
        {
            if (Currency == "ZAR") {
                return $"R{Math.Round(Value,2)}";
            }
            return $"{Value}{Currency}";
        }
    }
}