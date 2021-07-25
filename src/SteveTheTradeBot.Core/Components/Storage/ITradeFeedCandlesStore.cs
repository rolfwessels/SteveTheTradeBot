using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public interface ITradeFeedCandlesStore
    {
        Task<List<TradeFeedCandle>> FindCandlesByDate(string currencyPair, DateTime @from, DateTime to, PeriodSize periodSize, string feed = "valr", int skip = 0, int take = 1000000);
        Task<List<TradeFeedCandle>> FindRecentCandles(PeriodSize periodSize, DateTime beforeDate, int take, string currencyPair, string feed);

        Task<TradeFeedCandle> FindLatestCandle(string feed, string currencyPair, PeriodSize periodSize);
        Task<int> Remove(TradeFeedCandle foundCandle);
        IEnumerable<TradeFeedCandle> FindAllBetween( DateTime fromDate,  DateTime toDate, string feed,
            string currencyPair, PeriodSize periodSize, int batchSize = 1000);

        Task<int> AddRange(List<TradeFeedCandle> feedCandles);
        Task<List<TradeFeedCandle>> UpdateFeed(IEnumerable<KeyValuePair<DateTime, Dictionary<string, decimal?>>> store,
            string feed,
            string currencyPair, PeriodSize periodSize);

        Task<List<TradeFeedCandle>> FindBefore(DateTime startDate, string feed, string currencyPair,
            PeriodSize periodSize, int take);
    }
}