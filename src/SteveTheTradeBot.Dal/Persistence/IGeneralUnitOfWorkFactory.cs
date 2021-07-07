namespace SteveTheTradeBot.Dal.Persistence
{
    public interface IGeneralUnitOfWorkFactory
    {
        IGeneralUnitOfWork GetConnection();
        
    }
}