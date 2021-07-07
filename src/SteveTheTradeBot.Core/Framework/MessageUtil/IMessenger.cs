using System;
using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Framework.MessageUtil
{
    public interface IMessenger
    {
        Task Send<T>(T value);
        void Register<T>(object receiver, Action<T> action) where T : class;
        void Register(Type type, object receiver, Action<object> callBackToClient);
        void UnRegister<T>(object receiver);
        void UnRegister(Type type, object receiver);
        void UnRegister(object receiver);
    }
}