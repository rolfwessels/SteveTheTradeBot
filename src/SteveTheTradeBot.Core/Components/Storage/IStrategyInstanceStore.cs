using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.Dal.Persistence;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public interface IStrategyInstanceStore : IRepository<StrategyInstance>
    {
        Task RemoveByReference(string reference);
        Task<List<StrategyInstance>> FindActiveStrategies();
        Task<StrategyInstance> Update(StrategyInstance botDataStrategyInstance);
        Task<T> EnsureUpdate<T>(string id, Func<StrategyInstance, Task<T>> action);
    }
}