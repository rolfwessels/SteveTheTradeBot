using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
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

        public async Task PopulateNewThenOld(string currencyPair, CancellationToken token)
        {
            var (earliest, latest) = await _store.GetExistingRecords(currencyPair);

            var hasSomeData = earliest != null;
            if (hasSomeData) _log.Information($"Found existing records for {currencyPair} between {earliest.TradedAt} and {latest.TradedAt}.");
            await PopulateNewData(currencyPair, token);
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
            string? lastId = earliest.Id;
            while (saveChangesAsync != 0 && !token.IsCancellationRequested && lastId != null)
            {
                var trades = await _api.GetTradeHistory(currencyPair, lastId, BatchSize);
                var stopwatch = new Stopwatch().With(x=>x.Start());
                saveChangesAsync = await _store.AddRangeAndIgnoreDuplicates(trades.Select(x => x.ToDao()).ToList());
                _log.Information($"Saved {saveChangesAsync} new {currencyPair} items after {trades.Select(x=>x.TradedAt).LastOrDefault()} in  {stopwatch.Elapsed.ToShort()}");
                lastId = trades.LastOrDefault()?.Id;
            }
        }

        public async Task PopulateNewData(string currencyPair, CancellationToken token)
        {
            var trades = await _api.GetTradeHistory(currencyPair, 0, BatchSize);
            var saveChangesAsync = await _store.AddRangeAndIgnoreDuplicates(trades.Select(x => x.ToDao()).ToList());
            if (saveChangesAsync == 0) return;
            _log.Information($"Saved {saveChangesAsync} new {currencyPair} items after {trades.Select(x => x.TradedAt).LastOrDefault()}");
            while (saveChangesAsync == BatchSize && !token.IsCancellationRequested)
            {
                await Retry.Run(async () =>
                {
                    var stopwatch = new Stopwatch().With(x => x.Start());
                    trades = await _api.GetTradeHistory(currencyPair, trades.Last().Id, BatchSize);
                    saveChangesAsync = await _store.AddRangeAndIgnoreDuplicates(trades.Select(x => x.ToDao()).ToList());
                    _log.Information(
                        $"Saved {saveChangesAsync} new {currencyPair} items after {trades.Select(x => x.TradedAt).LastOrDefault()} in  {stopwatch.Elapsed.ToShort()}");
                }, token);

            }
        }

        

        
    }
}
