using System.Collections.Generic;
using System.Threading.Tasks;
using SteveTheTradeBot.Dal.Models.Users;

namespace SteveTheTradeBot.Core.Components.Users
{
    public interface IRoleManager
    {
        Task<Role> GetRoleByName(string name);
        Task<List<Role>> Get();
    }
}