using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.Mappers;

namespace SteveTheTradeBot.Core.Components.ThirdParty
{
    public interface IUpdateHistoricalData
    {
        Task StartUpdate();
    }

    public class UpdateHistoricalData : IUpdateHistoricalData
    {
        private readonly HistoricalDataApi _api;
        private readonly TradePersistenceStoreContext _tradeContext;

        public UpdateHistoricalData(HistoricalDataApi api , TradePersistenceStoreContext tradeContext)
        {
            _api = api;
            _tradeContext = tradeContext;
        }

        public async Task StartUpdate()
        {
            var trades = await _api.GetTradeHistory("BTCZAR");
            trades.ForEach(tradeResponse => _tradeContext.HistoricalTrades.Add(tradeResponse.ToDao()));  
            await _tradeContext.SaveChangesAsync();

        }
    }
}