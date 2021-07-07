using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Framework.Event
{
    public interface IEventStoreConnection
    {
        Task Append<T>(T value, CancellationToken token);
        IAsyncEnumerable<EventHolder> Read(CancellationToken token);
        void Register<T>();
        IObservable<EventHolder> ReadAndFollow(CancellationToken token);
    }
}