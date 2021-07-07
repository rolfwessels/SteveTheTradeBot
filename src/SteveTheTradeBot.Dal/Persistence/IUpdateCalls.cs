using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SteveTheTradeBot.Dal.Persistence
{
    public interface IUpdateCalls<T>
    {
        IUpdateCalls<T> Set<TType>(Expression<Func<T, TType>> expression, TType value);
        IUpdateCalls<T> Inc(Expression<Func<T, int>> expression, int value);
        IUpdateCalls<T> Inc(Expression<Func<T, long>> expression, long value);
        IUpdateCalls<T> Push<TT>(Expression<Func<T, IEnumerable<TT>>> expression, TT value);
        IUpdateCalls<T> SetOnInsert<TT>(Expression<Func<T, TT>> expression, TT value);
    }
}