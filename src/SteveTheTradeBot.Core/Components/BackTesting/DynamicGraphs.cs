using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public class DynamicGraphs
    {
        private readonly ITradePersistenceFactory _factory;
        private Lazy<Task<TradePersistenceStoreContext>> _contextLazy; 
        public DynamicGraphs(ITradePersistenceFactory factory)
        {
            _factory = factory;
            ResetLazy();
        }

        private void ResetLazy()
        {
            _contextLazy = new Lazy<Task<TradePersistenceStoreContext>>(() => _factory.GetTradePersistence());
        }

        public async Task Clear(string feedName)
        {
            
            var context = await _contextLazy.Value;
            var dynamicPlotters = context.DynamicPlots.Where(x=>x.Feed == feedName);
            context.DynamicPlots.RemoveRange(dynamicPlotters);
            await context.SaveChangesAsync();
        }

        public async Task Plot(string feedName, DateTime date, string label, decimal value)
        {
            var context = await _contextLazy.Value;
            await context.DynamicPlots.AddAsync(new DynamicPlotter()
            {
                Date = date,
                Feed = feedName,
                Label = label,
                Value = value,
            });
        }

        public async Task Flush()
        {
            var context = await _contextLazy.Value;
            await context.SaveChangesAsync();
            ResetLazy();
        }
    }
}