using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Serilog;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public class HistoricalDataPlayer : IHistoricalDataPlayer
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType); 
        private readonly TradeHistoryStore _tradeHistoryStore;
        private readonly TradePersistenceFactory _dataFactory;

        public HistoricalDataPlayer(TradeHistoryStore tradeHistoryStore)
        {
            _tradeHistoryStore = tradeHistoryStore;
        }

        #region Implementation of IHistoricalDataPlayer

        public IEnumerable<HistoricalTrade> ReadHistoricalTrades(DateTime @from, DateTime to, [EnumeratorCancellation] CancellationToken cancellationToken = default, int batchSize = 1000)
        {
            _log.Information($"ReadHistoricalTrades {from} {to}");
            List<HistoricalTrade> historicalTrades;
            var skip = 0;
            do
            {
                if (cancellationToken.IsCancellationRequested) break;
                historicalTrades = _tradeHistoryStore.FindByDate(@from, to, skip, batchSize).Result;
                skip += batchSize;
                foreach (var historicalTrade in historicalTrades.TakeWhile(historicalTrade => !cancellationToken.IsCancellationRequested))
                {
                    yield return historicalTrade;
                }

            } while (historicalTrades.Count != 0);
        }

        #endregion
    }

}