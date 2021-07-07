using System.Collections.Generic;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.BaseManagers;
using SteveTheTradeBot.Dal.Models.Users;
using SteveTheTradeBot.Dal.Persistence;

namespace SteveTheTradeBot.Core.Components.Users
{
    public class UserGrantLookup : BaseLookup<UserGrant>, IUserGrantLookup
    {
        public UserGrantLookup(IRepository<UserGrant> userGrant)
        {
            Repository = userGrant;
        }

        #region Overrides of BaseLookup<UserGrant>

        protected override IRepository<UserGrant> Repository { get; }

        #endregion

        #region Implementation of IUserGrantLookup

        public Task<UserGrant> GetByKey(string key)
        {
            return Repository.FindOne(x => x.Key == key);
        }

        public Task<List<UserGrant>> GetByUserId(string userId)
        {
            return Repository.Find(x => x.User.Id == userId);
        }

        public Task Insert(UserGrant userGrant)
        {
            return Repository.Add(userGrant);
        }

        public Task Delete(string id)
        {
            return Repository.Remove(x => x.Id == id);
        }

        #endregion
    }
}