using System;
using System.Collections.Generic;
using SteveTheTradeBot.Core.Components.Broker.Models;

namespace SteveTheTradeBot.Core.Components.ThirdParty.Valr
{
    public static class ValrFeeds
    {
        public static List<Feed> All => new List<Feed>() { new Feed(CurrencyPair.BTCZAR), new Feed(CurrencyPair.ETHZAR) };

        public class Feed
        {
            public string CurrencyPair { get; }

            public Feed(string currencyPair)
            {
                CurrencyPair = currencyPair;
            }
        }
    }

    
}