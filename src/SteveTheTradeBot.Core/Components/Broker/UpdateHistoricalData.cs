using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.Broker.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Core.Utils;
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

        public async Task StartUpdate(string currencyPair, CancellationToken token)
        {
            var (earliest, latest) = await _store.GetExistingRecords(currencyPair);

            var hasSomeData = earliest != null;
            if (hasSomeData) _log.Information($"Found existing records between {earliest.TradedAt} and {latest.TradedAt}.");
            await StartPopulatingData(currencyPair, token);
            if (hasSomeData)
            {
                await ProcessAllHistoricalData(currencyPair, earliest, token);
            }
        }

        public async Task UpdateHistory(string currencyPair, CancellationToken token)
        {
            var (earliest, _) = await _store.GetExistingRecords(currencyPair);

            var hasSomeData = earliest != null;
            if (!hasSomeData)
            {
                _log.Information("Adding initial batch.");
                var trades = await _api.GetTradeHistory(currencyPair, 0, 2);
                await _store.AddRangeAndIgnoreDuplicates(trades.Select(x => x.ToDao()).ToList());
                (earliest, _) = await _store.GetExistingRecords(currencyPair);
                hasSomeData = earliest != null;
            }
            if (hasSomeData)
            {
                await ProcessAllHistoricalData(currencyPair, earliest, token);
            }
        }

        private async Task ProcessAllHistoricalData(string currencyPair, HistoricalTrade earliest,
            CancellationToken token)
        {
            _log.Information($"Ensure we have the latest data after {earliest.TradedAt}.");
            var saveChangesAsync = BatchSize;
            var lastId = earliest.Id;
            while (saveChangesAsync != 0 && !token.IsCancellationRequested)
            {
                var trades = await _api.GetTradeHistory(currencyPair, lastId, BatchSize);
                var stopwatch = new Stopwatch().With(x=>x.Start());
                saveChangesAsync = await _store.AddRangeAndIgnoreDuplicates(trades.Select(x => x.ToDao()).ToList());
                _log.Information($"Saved {saveChangesAsync} new items after {trades.Select(x=>x.TradedAt).LastOrDefault()} in  {stopwatch.Elapsed.ToShort()}");
                lastId = trades.LastOrDefault()?.Id;
            }
        }

        private async Task StartPopulatingData(string currencyPair, CancellationToken token)
        {
            _log.Information("Start processing historical data");
            var trades = await _api.GetTradeHistory(currencyPair, 0, BatchSize);
            var saveChangesAsync = await _store.AddRangeAndIgnoreDuplicates(trades.Select(x => x.ToDao()).ToList());
            _log.Information($"Saved {saveChangesAsync} new items");
            while (saveChangesAsync == BatchSize && !token.IsCancellationRequested)
            {
                
                trades = await _api.GetTradeHistory(currencyPair, trades.Last().Id, BatchSize);
                saveChangesAsync = await _store.AddRangeAndIgnoreDuplicates(trades.Select(x => x.ToDao()).ToList());
                _log.Information($"Saved {saveChangesAsync} new items after {trades.Select(x => x.TradedAt).LastOrDefault()}");
            }
        }

        

        
    }
}
