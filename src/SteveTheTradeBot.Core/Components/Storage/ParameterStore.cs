using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public interface IParameterStore
    {
        Task Set(string key, string value);
        Task<string> Get(string key, string defaultValue);
        Task<DateTime> Get(string metricPopulate, DateTime defaultValue);
        Task Set(string key, in DateTime value);
    }

    public class ParameterStore : StoreBase<SimpleParam>, IParameterStore
    {
        public ParameterStore(ITradePersistenceFactory factory) : base(factory)
        {
        }

        public async Task Set(string key, string value)
        {
            await using var context = await _factory.GetTradePersistence();
            var found = context.SimpleParam.AsQueryable()
                .Where(x => x.Key == key).Take(1)
                .FirstOrDefault();
            if (found == null)
            {
                context.SimpleParam.Add(new SimpleParam {Key = key, Value = value});
            }
            else
            {
                found.Value = value;
                found.UpdateDate = DateTime.Now;
                context.SimpleParam.Update(found);
            }

            await context.SaveChangesAsync();
        }

        #region Overrides of StoreBase<SimpleParam>

        protected override DbSet<SimpleParam> DbSet(TradePersistenceStoreContext context)
        {
            return context.SimpleParam;
        }

        #endregion

        public async Task<string> Get(string key, string defaultValue)
        {
            await using var context = await _factory.GetTradePersistence();

            var firstOrDefault = context.SimpleParam.AsQueryable()
                .Where(x => x.Key == key).Take(1)
                .FirstOrDefault();

            return firstOrDefault?.Value ?? defaultValue;
        }

        public async Task<DateTime> Get(string key, DateTime defaultValue)
        {
            var value = await Get(key, "_");
            if (value != "_" && DateTime.TryParse(value, out var date))
            {
                return date;
            }

            return defaultValue;
        }

        public Task Set(string key, in DateTime value)
        {
            return Set(key, value.ToIsoDateString());
        }
    }
}