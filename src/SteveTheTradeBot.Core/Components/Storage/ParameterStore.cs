using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public interface IParamsStoreSimple
    {
        Task Set(string key, string defaultValue);
        Task<string> Get(string key, string defaultValue);
    }

    public interface IParameterStore : IParamsStoreSimple
    {
        
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

        

       
    }
}