using System;

namespace SteveTheTradeBot.Core.Framework.Event
{
    public class EventHolder
    {
        public string EventType { get; }
        public object Value { get; }

        protected EventHolder(string eventType, object value)
        {
            EventType = eventType;
            Value = value;
        }

        public static EventHolder From(string eventType, object value)
        {
            Type generic = typeof(EventHolderTyped<>);
            Type constructed = generic.MakeGenericType(value.GetType());
            return Activator.CreateInstance(constructed, eventType, value) as EventHolder;
        }
    }
}