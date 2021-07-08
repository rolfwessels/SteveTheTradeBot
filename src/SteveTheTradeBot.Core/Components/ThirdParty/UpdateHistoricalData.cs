using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.ThirdParty
{
    public class CurrencyPair
    {
        public const string BTCZAR = "BTCZAR";
    }

    public class UpdateHistoricalData : IUpdateHistoricalData
    {
        
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IHistoricalDataApi _api;
        private readonly TradePersistenceFactory _factory;
        public int BatchSize { get; set; } = 100;
        public UpdateHistoricalData(IHistoricalDataApi api , TradePersistenceFactory factory)
        {
            _api = api;
            _factory = factory;
        }

        public async Task StartUpdate(string currencyPair)
        {
            var (earliest, latest) = await GetExistingRecords();

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
            while (saveChangesAsync == BatchSize)
            {
                _log.Information($"Continue processing from {lastId}");
                var trades = await _api.GetTradeHistory(currencyPair, lastId, BatchSize);
                saveChangesAsync = await AddToListAndIgnoreDuplicates(trades);
                _log.Information($"Saved {saveChangesAsync} new items");
                lastId = trades.LastOrDefault()?.Id;
            }
        }

        private async Task StartPopulatingData(string currencyPair)
        {
            _log.Information("Start processing historical data");
            var trades = await _api.GetTradeHistory(currencyPair, 0, BatchSize);

            var saveChangesAsync = await AddToListAndIgnoreDuplicates(trades);
            _log.Information($"Saved {saveChangesAsync} new items");
            while (saveChangesAsync == BatchSize)
            {
                _log.Information($"Continue processing from {trades.Last().Id}");
                trades = await _api.GetTradeHistory(currencyPair, trades.Last().Id, BatchSize);
                saveChangesAsync = await AddToListAndIgnoreDuplicates(trades);
                _log.Information($"Saved {saveChangesAsync} new items");
            }
        }

        private async Task<(HistoricalTrade earliest, HistoricalTrade latest)> GetExistingRecords()
        {
            var context = await _factory.GetTradePersistence();
            var earliest = context.HistoricalTrades.AsQueryable().OrderBy(x => x.TradedAt).Take(1).FirstOrDefault();
            var latest = context.HistoricalTrades.AsQueryable().OrderByDescending(x => x.TradedAt).Take(1).FirstOrDefault();
            return (earliest, latest);
        }

        private async Task<int> AddToListAndIgnoreDuplicates(TradeResponseDto[] trades)
        {
            var tradePersistenceStoreContext = await _factory.GetTradePersistence();
            var historicalTrades = trades.Select(x => x.ToDao()).ToList();
            tradePersistenceStoreContext.HistoricalTrades.AddRange(historicalTrades);
            try
            {
                return await tradePersistenceStoreContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                var ids = trades.Select(x=>x.Id).ToArray();
                var exists = tradePersistenceStoreContext.HistoricalTrades.AsQueryable().Where(x=> ids.Contains(x.Id)).Select(x=>x.Id).ToList();
                tradePersistenceStoreContext = await _factory.GetTradePersistence();
                tradePersistenceStoreContext.HistoricalTrades.AddRange(historicalTrades.Where(x => !exists.Contains(x.Id)));
                return  await tradePersistenceStoreContext.SaveChangesAsync();
            }
        }
    }
}
