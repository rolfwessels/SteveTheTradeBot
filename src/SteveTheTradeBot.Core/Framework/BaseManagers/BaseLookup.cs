using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Dal.Models.Base;
using SteveTheTradeBot.Dal.Persistence;
using SteveTheTradeBot.Dal.Validation;

namespace SteveTheTradeBot.Core.Framework.BaseManagers
{
   

    public abstract class BaseLookup<T> :  IBaseLookup<T> where T : BaseDalModelWithId
    {
        
        protected abstract IRepository<T> Repository { get; }


        public Task<List<T>> Get(Expression<Func<T, bool>> filter)
        {
            return Repository.Find(filter);
        }

        public Task<List<T>> Get()
        {
            return Repository.Find(x => true);
        }

        public virtual Task<T> GetById(string id)
        {
            return Repository.FindOne(x => x.Id == id);
        }

        public Task<IQueryable<T>> Query(Func<IQueryable<T>, IQueryable<T>> query)
        {
            return Task.Run(() => query(Repository.Query()));
        }

        public IQueryable<T> Query()
        {
            return Repository.Query();
        }
    }
}