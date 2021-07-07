using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Dal.Models.Base;
using SteveTheTradeBot.Dal.Models.Projects;
using SteveTheTradeBot.Dal.Models.SystemEvents;
using SteveTheTradeBot.Dal.Models.Users;
using SteveTheTradeBot.Dal.Persistence;

namespace SteveTheTradeBot.Dal.InMemoryCollections
{
    public class InMemoryGeneralUnitOfWork : IGeneralUnitOfWork
    {
        public InMemoryGeneralUnitOfWork()
        {
            Users = new FakeRepository<User>();
            Projects = new FakeRepository<Project>();
            UserGrants = new FakeRepository<UserGrant>();
            SystemCommands = new FakeRepository<SystemCommand>();
            SystemEvents = new FakeRepository<SystemEvent>();
        }

        #region IGeneralUnitOfWork Members

        #region Implementation of IDisposable

        public void Dispose()
        {
        }

        #endregion

        #endregion


        #region Implementation of IGeneralUnitOfWork

        public IRepository<User> Users { get; }
        public IRepository<Project> Projects { get; }
        public IRepository<UserGrant> UserGrants { get; }
        public IRepository<SystemCommand> SystemCommands { get; set; }
        public IRepository<SystemEvent> SystemEvents { get; set; }

        #endregion
    }


    public class FakeRepository<T> : IRepository<T> where T : IBaseDalModel
    {
        private readonly List<T> _internalDataList;

        public FakeRepository()
        {
            _internalDataList = new List<T>();
        }

        public List<T> InternalDataList => _internalDataList;

        #region Private Methods

        private void AddAndSetUpdateDate(T entity)
        {
            _internalDataList.Add(entity.DynamicCastTo<T>());
            entity.UpdateDate = DateTime.Now;
        }

        #endregion

        #region Implementation of IRepository<T>

        public IQueryable<T> Query()
        {
            return _internalDataList.AsQueryable();
        }

        public Task<T> Add(T entity)
        {
            entity.CreateDate = DateTime.Now;
            if (entity is IBaseDalModelWithId baseDalModelWithId && string.IsNullOrEmpty(baseDalModelWithId.Id))
                baseDalModelWithId.Id = BuildId();
            AddAndSetUpdateDate(entity);
            return Task.FromResult(entity);
        }

        private static string BuildId()
        {
            return Guid.NewGuid().ToString("n").ToLower().Substring(0, 24);
        }


        public Task<IEnumerable<T>> AddRange(IEnumerable<T> entities)
        {
            var addRange = entities as T[] ?? entities.ToArray();
            foreach (var entity in addRange) Add(entity);

            return Task.FromResult(entities);
        }


        public Task<long> Update<TType>(Expression<Func<T, bool>> filter, Expression<Func<T, TType>> update,
            TType value)
            where TType : class
        {
            var enumerable = _internalDataList.Where(filter.Compile()).ToArray();
            foreach (var v in enumerable) ReflectionHelper.ExpressionToAssign(v, update, value);

            return Task.FromResult(enumerable.LongCount());
        }

        public Task<bool> Remove(Expression<Func<T, bool>> filter)
        {
            var array = _internalDataList.Where(filter.Compile()).ToArray();
            array.ForEach(x => _internalDataList.Remove(x));
            return Task.FromResult(array.Length > 0);
        }

        public async Task<List<T>> Find(Expression<Func<T, bool>> filter)
        {
            var fromResult = await FindInternal(filter);
            return fromResult.DynamicCastTo<List<T>>();
        }

        private Task<List<T>> FindInternal(Expression<Func<T, bool>> filter)
        {
            return Task.FromResult(_internalDataList.Where(filter.Compile()).ToList());
        }

        public async Task<T> FindOne(Expression<Func<T, bool>> filter)
        {
            var list = await FindInternal(filter);
            return list.DynamicCastTo<List<T>>().FirstOrDefault();
        }

        public Task<long> Count()
        {
            return Task.FromResult(_internalDataList.LongCount());
        }

        public Task<long> Count(Expression<Func<T, bool>> filter)
        {
            return Task.FromResult(_internalDataList.Where(filter.Compile()).LongCount());
        }

        public async Task<long> UpdateMany(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            var list = await FindInternal(filter);
            var updateCalls = new UpdateCalls<T>(list);
            upd(updateCalls);
            return list.LongCount();
        }

        public async Task<long> UpdateOne(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            var list = (await FindInternal(filter)).Take(1).ToList();
            var updateCalls = new UpdateCalls<T>(list);
            upd(updateCalls);
            return list.LongCount();
        }

        public async Task<long> Upsert(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd)
        {
            var list = (await FindInternal(filter)).Take(1).ToList();
            if (!list.Any())
            {
                var instance = Activator.CreateInstance<T>();
                _internalDataList.Add(instance);
                list.Add(instance);
            }

            var updateCalls = new UpdateCalls<T>(list);
            upd(updateCalls);
            return list.Count;
        }

        public async Task<T> FindOneAndUpdate(Expression<Func<T, bool>> filter, Action<IUpdateCalls<T>> upd,
            bool isUpsert = false)
        {
            var list = (await FindInternal(filter)).Take(1).ToList();
            if (isUpsert && !list.Any())
            {
                var instance = Activator.CreateInstance<T>();
                _internalDataList.Add(instance);
                list.Add(instance);
            }

            var updateCalls = new UpdateCalls<T>(list);
            upd(updateCalls);
            return list.First();
        }

        public class UpdateCalls<TClass> : IUpdateCalls<TClass>
        {
            private readonly List<TClass> _list;

            public UpdateCalls(List<TClass> list)
            {
                _list = list;
            }

            #region Implementation of IUpdateCalls<TClass>

            public IUpdateCalls<TClass> Set<TType>(Expression<Func<TClass, TType>> expression, TType value)
            {
                foreach (var item in _list) AssignNewValue(item, expression, value);

                return this;
            }

            public IUpdateCalls<TClass> SetOnInsert<TT>(Expression<Func<TClass, TT>> expression, TT value)
            {
                foreach (var item in _list) AssignNewValue(item, expression, value);

                return this;
            }

            public IUpdateCalls<TClass> Inc(Expression<Func<TClass, int>> expression, int value)
            {
                var compile = expression.Compile();
                foreach (var item in _list)
                {
                    var i = compile(item);
                    AssignNewValue(item, expression, i += value);
                }

                return this;
            }

            public IUpdateCalls<TClass> Inc(Expression<Func<TClass, long>> expression, long value)
            {
                var compile = expression.Compile();
                foreach (var item in _list)
                {
                    var i = compile(item);
                    AssignNewValue(item, expression, i += value);
                }

                return this;
            }

            public IUpdateCalls<TClass> Push<TT>(Expression<Func<TClass, IEnumerable<TT>>> expression, TT value)
            {
                throw new NotImplementedException();
            }

            #endregion


            public static void AssignNewValue<TObj, TValue>(TObj obj, Expression<Func<TObj, TValue>> expression,
                TValue value)
            {
                ReflectionHelper.ExpressionToAssign(obj, expression, value);
            }
        }

        public Task<T> Update(Expression<Func<T, bool>> filter, T entity)
        {
            Remove(filter);
            AddAndSetUpdateDate(entity);
            return Task.FromResult(entity);
        }

        public bool Remove(T entity)
        {
            if (entity is IBaseDalModelWithId baseDalModelWithId)
            {
                var baseDalModelWithIds =
                    _internalDataList.Cast<IBaseDalModelWithId>().FirstOrDefault(x => x.Id == baseDalModelWithId.Id);
                if (baseDalModelWithIds != null)
                {
                    _internalDataList.Remove((T) baseDalModelWithIds);
                    return true;
                }
            }

            return _internalDataList.Remove(entity);
        }

        public T Update(T entity, object t)
        {
            Remove(entity);
            AddAndSetUpdateDate(entity);
            return entity;
        }

        #endregion
    }
}