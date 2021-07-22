using System;

namespace SteveTheTradeBot.Core.Components.Broker.Models
{
    public static class SidePicker
    {
        public static string SideOut(this string currencyPair,Side side)
        {
            switch (side)
            {
                case Side.Sell:
                    return currencyPair.Substring(0, 3);
                case Side.Buy:
                    return currencyPair.Substring(3, 3);
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        } 
        
        public static string SideIn(this string currencyPair,Side side)
        {
            switch (side)
            {
                case Side.Buy:
                    return currencyPair.Substring(0, 3);
                case Side.Sell:
                    return currencyPair.Substring(3, 3);
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

    }
}