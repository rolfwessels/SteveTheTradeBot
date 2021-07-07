using System.Threading.Tasks;

namespace SteveTheTradeBot.Shared.Interfaces.Base
{
    public interface ICrudController<TModel, in TDetailModel>
    {
        Task<TModel> GetById(string id);
    }
}