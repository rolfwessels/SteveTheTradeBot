using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public interface IDynamicGraphs
    {
        Task Clear(string feedName);
        Task Plot(string feedName, DateTime date, string label, decimal value);
        Task Flush();
    }

    public class DynamicGraphs : IDynamicGraphs
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
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
            var dynamicPlotters = context.DynamicPlots.Where(x => x.Feed == feedName);
            context.DynamicPlots.RemoveRange(dynamicPlotters);
            await context.SaveChangesAsync();
        }

        public async Task Plot(string feedName, DateTime date, string label, decimal value)
        {
            var context = await _contextLazy.Value;
            try
            {
                await context.DynamicPlots.AddAsync(new DynamicPlotter()
                {
                    Date = date,
                    Feed = feedName,
                    Label = label,
                    Value = value,
                });
            }
            catch (Exception e)
            {
                _log.Warning(e, $"DynamicGraphs:Plot add {e.Message}");
            }
        }

        public async Task Flush()
        {
            var context = await _contextLazy.Value;
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _log.Warning(e, $"DynamicGraphs:Plot save failed {e.Message}");
            }
            finally
            {
                ResetLazy();
            }
        }
    }
}