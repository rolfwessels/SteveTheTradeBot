using System.Collections.Generic;
using System.Threading.Tasks;
using SteveTheTradeBot.Shared.Models.Shared;

namespace SteveTheTradeBot.Shared.Interfaces.Base
{
    public interface IBaseStandardLookups<TDetails, TModelReference> : IBaseControllerLookups<TDetails, TModelReference>
    {
        Task<PagedResult<TModelReference>> GetPaged(string oDataQuery);
        Task<IEnumerable<TModelReference>> Get(string oDataQuery);
        Task<IEnumerable<TDetails>> GetDetail(string oDataQuery);
        Task<PagedResult<TDetails>> GetDetailPaged(string oDataQuery);
    }
}