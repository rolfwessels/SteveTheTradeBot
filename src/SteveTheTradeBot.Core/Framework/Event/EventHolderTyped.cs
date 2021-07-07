namespace SteveTheTradeBot.Core.Framework.Event
{
    public class EventHolderTyped<T>: EventHolder
    {
        public EventHolderTyped(string eventType, object value) : base(eventType, value)
        {
        }

        public T Typed => (T) Value;

        
    }
}