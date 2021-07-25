using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public interface ITradeHistoryStore
    {
        Task<(HistoricalTrade earliest, HistoricalTrade latest)> GetExistingRecords(string currencyPair);
        Task<int> AddRangeAndIgnoreDuplicates(List<HistoricalTrade> trades);
        Task<List<HistoricalTrade>> FindById(params string[] ids);
        Task<List<HistoricalTrade>> FindByDate(string currencyPair, DateTime @from, DateTime to, int skip = 0,
            int take = 1000000);
        }
}