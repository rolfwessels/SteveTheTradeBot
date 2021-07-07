namespace SteveTheTradeBot.Dal.Models.Base
{
    public interface IBaseDalModelWithId : IBaseDalModel
    {
        string Id { get; set; }
    }
}