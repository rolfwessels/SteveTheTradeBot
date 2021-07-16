﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Serilog;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker
{
    public class HistoricalDataPlayer : IHistoricalDataPlayer
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType); 
        private readonly ITradeHistoryStore _tradeHistoryStore;
        private readonly TradePersistenceFactory _dataFactory;

        public HistoricalDataPlayer(ITradeHistoryStore tradeHistoryStore)
        {
            _tradeHistoryStore = tradeHistoryStore;
        }

        #region Implementation of IHistoricalDataPlayer

        public IEnumerable<HistoricalTrade> ReadHistoricalTrades(DateTime @from, DateTime to, CancellationToken cancellationToken = default, int batchSize = 1000)
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

        public IEnumerable<TradeFeedCandle> ReadHistoricalData(DateTime @from, DateTime to, PeriodSize periodSize, CancellationToken cancellationToken = default, int batchSize = 1000)
        {
            _log.Information($"ReadHistoricalTrades {from} {to}");
            List<TradeFeedCandle> historicalTrades;
            var skip = 0;
            do
            {
                if (cancellationToken.IsCancellationRequested) break;
                historicalTrades = _tradeHistoryStore.FindCandlesByDate(@from, to,periodSize,skip:skip,take: batchSize).Result;
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