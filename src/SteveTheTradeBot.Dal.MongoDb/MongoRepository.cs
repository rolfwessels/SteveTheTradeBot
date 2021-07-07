using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SteveTheTradeBot.Dal.Models.Base;
using SteveTheTradeBot.Dal.Persistence;
using MongoDB.Driver;

namespace SteveTheTradeBot.Dal.MongoDb
{
    public class MongoRepository<T> : IRepository<T> where T : IBaseDalModel
    {
        public MongoRepository(IMongoDatabase database)
        {
            Collection = database.GetCollection<T>(typeof(T).Name);
        }

        public IMongoCollection<T> Collection { get; }

        public Task<List<T>> Find()
        {
            return Find(x => true);
        }

        #region Implementation of IRepository<T>

        public IQueryable<T> Query()
        {
            return Collection.AsQueryable();
        }

        public async Task<T> Add(T entity)
        {
            entity.CreateDate = DateTime.Now;
            entity.UpdateDate = DateTime.Now;
            await Collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<T>> AddRange(IEnumerable<T> entities)
        {
            var enumerable = entities as IList<T> ?? entities.ToList();
            foreach (var entity in enumerable)
            {
                entity.CreateDate = DateTime.Now;
                entity.UpdateDate = DateTime.Now;
            }

            await Collection.InsertManyAsync(enumerable);

            return enumerable;
        }


        public async Task<T> Update(Expression<Func<T, bool>> filter, T entity)
        {
            entity.UpdateDate = DateTime.Now;
            await Collection.ReplaceOneAsync(Builders<T>.Filter.Where(filter), entity);
            return entity;
        }

        public async Task<long> UpdateMany(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            var filterDefinition = Builders<T>.Filter.Where(filter);
            var updateDefinition = Builders<T>.Update.CurrentDate(x => x.UpdateDate);
            var mongoUpdate = new MongoUpdate<T>(updateDefinition);
            upd(mongoUpdate);
            var result = await Collection.UpdateManyAsync(filterDefinition, mongoUpdate.UpdateDefinition);
            return result.ModifiedCount;
        }

        public async Task<long> UpdateOne(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            var filterDefinition = Builders<T>.Filter.Where(filter);
            var updateDefinition = Builders<T>.Update.CurrentDate(x => x.UpdateDate);
            var mongoUpdate = new MongoUpdate<T>(updateDefinition);
            upd(mongoUpdate);
            var result = await Collection.UpdateOneAsync(filterDefinition, mongoUpdate.UpdateDefinition);
            return result.ModifiedCount;
        }

        public async Task<long> Upsert(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            var filterDefinition = Builders<T>.Filter.Where(filter);
            var updateDefinition = Builders<T>.Update.CurrentDate(x => x.UpdateDate);
            var mongoUpdate = new MongoUpdate<T>(updateDefinition);
            upd(mongoUpdate);
            var result = await Collection.UpdateOneAsync(filterDefinition, mongoUpdate.UpdateDefinition,
                new UpdateOptions {IsUpsert = true});
            return result.ModifiedCount;
        }

        public async Task<T> FindOneAndUpdate(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd,
            bool isUpsert = true)
        {
            var filterDefinition = Builders<T>.Filter.Where(filter);
            var updateDefinition = Builders<T>.Update.CurrentDate(x => x.UpdateDate);
            var mongoUpdate = new MongoUpdate<T>(updateDefinition);
            upd(mongoUpdate);
            var result = await Collection.FindOneAndUpdateAsync(filterDefinition, mongoUpdate.UpdateDefinition,
                new FindOneAndUpdateOptions<T> {IsUpsert = isUpsert, ReturnDocument = ReturnDocument.After});
            return result;
        }

        public class MongoUpdate<TType> : IUpdateCalls<TType>
        {
            public MongoUpdate(UpdateDefinition<TType> updateDefinition)
            {
                UpdateDefinition = updateDefinition;
            }

            public UpdateDefinition<TType> UpdateDefinition { get; private set; }

            #region IUpdateCalls<TType> Members

            public IUpdateCalls<TType> Set<TT>(Expression<Func<TType, TT>> expression, TT value)
            {
                UpdateDefinition = UpdateDefinition.Set(expression, value);
                return this;
            }

            public IUpdateCalls<TType> SetOnInsert<TT>(Expression<Func<TType, TT>> expression, TT value)
            {
                UpdateDefinition = UpdateDefinition.SetOnInsert(expression, value);
                return this;
            }

            public IUpdateCalls<TType> Inc(Expression<Func<TType, int>> expression, int value)
            {
                UpdateDefinition = UpdateDefinition.Inc(expression, value);
                return this;
            }

            public IUpdateCalls<TType> Inc(Expression<Func<TType, long>> expression, long value)
            {
                UpdateDefinition = UpdateDefinition.Inc(expression, value);
                return this;
            }

            public IUpdateCalls<TType> Push<TT>(Expression<Func<TType, IEnumerable<TT>>> expression, TT value)
            {
                UpdateDefinition = UpdateDefinition.Push(expression, value);
                return this;
            }

            #endregion
        }

        public async Task<bool> Remove(Expression<Func<T, bool>> filter)
        {
            var deleteResult = await Collection.DeleteOneAsync(filter);
            return deleteResult.DeletedCount > 0;
        }

        public Task<List<T>> Find(Expression<Func<T, bool>> filter)
        {
            return Collection.Find(Builders<T>.Filter.Where(filter)).ToListAsync();
        }

        public Task<T> FindOne(Expression<Func<T, bool>> filter)
        {
            return Collection.Find(Builders<T>.Filter.Where(filter)).FirstOrDefaultAsync();
        }


        public Task<long> Count()
        {
            return Count(x => true);
        }

        public Task<long> Count(Expression<Func<T, bool>> filter)
        {
            return Collection.CountDocumentsAsync(Builders<T>.Filter.Where(filter));
        }

        #endregion
    }
}