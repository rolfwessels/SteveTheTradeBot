namespace SteveTheTradeBot.Shared.Models.Shared
{
    public class ValueUpdateModel<T>
    {
        public ValueUpdateModel(T value, UpdateTypeCodes updateType)
        {
            Value = value;
            UpdateType = updateType;
        }

        public T Value { get; }

        public UpdateTypeCodes UpdateType { get; }
    }
}