using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.BaseManagers;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Dal.Models.Users;

namespace SteveTheTradeBot.Core.Components.Users
{
    public interface IUserLookup : IBaseLookup<User>
    {
        Task<User> GetUserByEmail(string email);
        Task<PagedList<User>> GetPagedUsers(UserPagedLookupOptions options);
        Task<User> GetUserByEmailAndPassword(string contextUserName, string contextPassword);
    }
}