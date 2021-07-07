using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Core.Framework.MessageUtil.Models
{
    public class DalUpdateMessage<T>
    {
        public DalUpdateMessage(T value, UpdateTypes updateType)
        {
            Value = value;
            UpdateType = updateType;
        }

        public T Value { get; }

        public UpdateTypes UpdateType { get; }

        public override string ToString()
        {
            return $"UpdateType: {UpdateType}, Value: {Value}";
        }
    }
}