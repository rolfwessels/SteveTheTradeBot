namespace SteveTheTradeBot.Dal.Persistence
{
    public interface IIdGenerator
    {
        string NewId { get; }
    }
}