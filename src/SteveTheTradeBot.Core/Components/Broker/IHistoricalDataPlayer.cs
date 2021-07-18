using System;
using System.Collections.Generic;
using System.Threading;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public interface IHistoricalDataPlayer
    {
        IEnumerable<HistoricalTrade> ReadHistoricalTrades(string currencyPair, DateTime @from, DateTime to,
            CancellationToken cancellationToken = default, int batchSize = 1000);
    }
}