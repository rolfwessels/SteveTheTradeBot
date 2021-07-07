using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SteveTheTradeBot.Core.Framework.MessageUtil
{
    public class Messenger : IMessenger
    {
        private static readonly Lazy<Messenger> _messenger;
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<WeakReference, Action<object>>> _dictionary;

        static Messenger()
        {
            _messenger = new Lazy<Messenger>(() => new Messenger());
        }

        public Messenger()
        {
            _dictionary = new ConcurrentDictionary<Type, ConcurrentDictionary<WeakReference, Action<object>>>();
        }

        public static IMessenger Default => _messenger.Value;

        #region IMessenger Members

        public Task Send<T>(T value)
        {
            return Task.Run(() =>
            {
                if (!_dictionary.TryGetValue(typeof(T), out var type)) return;
                foreach (var reference in type)
                    if (reference.Key.IsAlive)
                        reference.Value(value);
                    else
                        type.TryRemove(reference.Key, out _);
            });
        }

        public void Register<T>(object receiver, Action<T> action) where T : class
        {
            void Value(object t)
            {
                action(t as T);
            }
            Register(typeof(T), receiver, Value);
        }

        public void Register(Type type, object receiver, Action<object> action)
        {
            var weakReference = new WeakReference(receiver);
            var references = _dictionary.GetOrAdd(type, t => new ConcurrentDictionary<WeakReference, Action<object>>());
            references.AddOrUpdate(weakReference, action, (k, v) => action);
        }

        public void UnRegister<T>(object receiver)
        {
            UnRegister(typeof(T), receiver);
        }

        public void UnRegister(Type type, object receiver)
        {
            if (_dictionary.TryGetValue(type, out var typeFound))
                foreach (var key in typeFound.Keys)
                {
                    if (!key.IsAlive)
                    {
                        typeFound.TryRemove(key, out _);
                        continue;
                    }

                    if (key.Target == receiver) typeFound.TryRemove(key, out _);
                }
        }

        public void UnRegister(object receiver)
        {
            foreach (var type in _dictionary.Values)
            foreach (var key in type.Keys)
            {
                if (!key.IsAlive)
                {
                    type.TryRemove(key, out _);
                    continue;
                }

                if (key.Target == receiver) type.TryRemove(key, out _);
            }
        }

        #endregion
    }
}