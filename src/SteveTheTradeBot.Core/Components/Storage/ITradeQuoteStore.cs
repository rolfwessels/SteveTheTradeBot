using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public interface ITradeQuoteStore
    {
        Task<List<TradeQuote>> FindByDate(string currencyPair, DateTime @from, DateTime to, PeriodSize periodSize, string feed = "valr", int skip = 0, int take = 1000000);
        Task<List<TradeQuote>> FindRecent(PeriodSize periodSize, DateTime beforeDate, int take, string currencyPair, string feed);

        Task<TradeQuote> FindLatest(string feed, string currencyPair, PeriodSize periodSize);
        Task<int> Remove(TradeQuote foundCandle);
        IEnumerable<TradeQuote> FindAllBetween( DateTime fromDate,  DateTime toDate, string feed,
            string currencyPair, PeriodSize periodSize, int batchSize = 1000);

        Task<int> AddRange(List<TradeQuote> feedQuotes);
        Task<List<TradeQuote>> UpdateFeed(IEnumerable<KeyValuePair<DateTime, Dictionary<string, decimal?>>> store,
            string feed,
            string currencyPair, PeriodSize periodSize);

        Task<List<TradeQuote>> FindBefore(DateTime startDate, string feed, string currencyPair,
            PeriodSize periodSize, int take);

        Task Populate(CancellationToken token, string feedCurrencyPair, string feedName, PeriodSize periodSize);
    }
}