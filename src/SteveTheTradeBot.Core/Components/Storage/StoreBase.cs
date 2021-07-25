using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Base;
using SteveTheTradeBot.Dal.Persistence;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public abstract class StoreBase<T> : IRepository<T> where T : BaseDalModel 
    {
        
        protected readonly ITradePersistenceFactory _factory;
        private int DefaultMax = 1000;

        protected StoreBase(ITradePersistenceFactory factory)
        {
            _factory = factory;
        }

        

        protected abstract DbSet<T> DbSet(TradePersistenceStoreContext context);

        public async Task<int> Remove(T foundCandle)
        {
            await using var context = await _factory.GetTradePersistence();
            DbSet(context).Remove(foundCandle);
            return context.SaveChanges();
        }

        #region Implementation of IRepository<T>

        public IQueryable<T> Query()
        {
            var context = _factory.GetTradePersistence().Result;
            return DbSet(context).AsQueryable();
        }

        public async Task<T> Add(T entity)
        {
            await using var context = await _factory.GetTradePersistence();
            DbSet(context).Add(entity);
            context.SaveChanges();
            return entity;
        }

        public async Task<IEnumerable<T>> AddRange(IEnumerable<T> entities)
        {
            await using var context = await _factory.GetTradePersistence();
            var baseDalModels = entities as T[] ?? entities.ToArray();
            DbSet(context).AddRange(baseDalModels);
            context.SaveChanges();
            return baseDalModels;
        }


        public async Task<T> Update(T entity)
        {
            await using var context = await _factory.GetTradePersistence();
            DbSet(context).Update(entity);
            context.SaveChanges();
            return entity;
        }

        public async Task<T> Update(Expression<Func<T, bool>> filter, T entity)
        {
            await using var context = await _factory.GetTradePersistence();
            var list = DbSet(context).AsQueryable().Where(filter).Take(1).ToList();
            if (!list.Any()) throw new ArgumentException("Could not find entity to update.");
            context.Update(entity);
            context.SaveChanges();
            return entity;
        }

        public Task<bool> Remove(Expression<Func<T, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public async Task<List<T>> Find(Expression<Func<T, bool>> filter)
        {
            await using var context = await _factory.GetTradePersistence();
            return DbSet(context).AsQueryable().Where(filter).Take(DefaultMax).ToList();
        }

        public Task<T> FindOne(Expression<Func<T, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public async Task<long> Count()
        {
            await using var context = await _factory.GetTradePersistence();
            return DbSet(context).Count();
        }

        public Task<long> Count(Expression<Func<T, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<long> UpdateMany(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            throw new NotImplementedException();
        }

        public Task<long> UpdateOne(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            throw new NotImplementedException();
        }

        public Task<long> Upsert(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            throw new NotImplementedException();
        }

        public Task<T> FindOneAndUpdate(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd, bool isUpsert = false)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}