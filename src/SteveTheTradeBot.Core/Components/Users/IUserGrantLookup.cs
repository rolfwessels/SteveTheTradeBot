using System.Collections.Generic;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.BaseManagers;
using SteveTheTradeBot.Dal.Models.Users;

namespace SteveTheTradeBot.Core.Components.Users
{
    public interface IUserGrantLookup : IBaseLookup<UserGrant>
    {
        Task<UserGrant> GetByKey(string key);
        Task<List<UserGrant>> GetByUserId(string userId);
        Task Insert(UserGrant userGrant);
        Task Delete(string id);
    }
}