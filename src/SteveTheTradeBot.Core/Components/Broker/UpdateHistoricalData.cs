using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Broker
{

    public class UpdateHistoricalData : IUpdateHistoricalData
    {
        
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IHistoricalDataApi _api;
        private readonly ITradeHistoryStore _store;
        public int BatchSize { get; set; } = 100;
        public UpdateHistoricalData(IHistoricalDataApi api , ITradeHistoryStore store)
        {
            _api = api;
            _store = store;
        }

        public async Task StartUpdate(string currencyPair)
        {
            var (earliest, latest) = await _store.GetExistingRecords();

            var hasSomeData = earliest != null;
            if (hasSomeData) _log.Information($"Found existing records between {earliest.TradedAt} and {latest.TradedAt}.");
            await StartPopulatingData(currencyPair);
            if (hasSomeData)
            {
                await ProcessAllHistoricalData(currencyPair, earliest);
            }
        }

        private async Task ProcessAllHistoricalData(string currencyPair, HistoricalTrade earliest)
        {
            _log.Information($"Ensure we have the latest data after {earliest.TradedAt}.");
            var saveChangesAsync = BatchSize;
            var lastId = earliest.Id;
            while (saveChangesAsync != 0)
            {
                _log.Information($"Continue processing from {lastId}");
                var trades = await _api.GetTradeHistory(currencyPair, lastId, BatchSize);
                saveChangesAsync = await _store.AddRangeAndIgnoreDuplicates(trades.Select(x => x.ToDao()).ToList());
                _log.Information($"Saved {saveChangesAsync} new items");
                lastId = trades.LastOrDefault()?.Id;
            }
        }

        private async Task StartPopulatingData(string currencyPair)
        {
            _log.Information("Start processing historical data");
            var trades = await _api.GetTradeHistory(currencyPair, 0, BatchSize);
            var saveChangesAsync = await _store.AddRangeAndIgnoreDuplicates(trades.Select(x => x.ToDao()).ToList());
            _log.Information($"Saved {saveChangesAsync} new items");
            while (saveChangesAsync == BatchSize)
            {
                _log.Information($"Continue processing from {trades.Last().Id}");
                trades = await _api.GetTradeHistory(currencyPair, trades.Last().Id, BatchSize);
                saveChangesAsync = await _store.AddRangeAndIgnoreDuplicates(trades.Select(x => x.ToDao()).ToList());
                _log.Information($"Saved {saveChangesAsync} new items");
            }
        }

        

        
    }
}
