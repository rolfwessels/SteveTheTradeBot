using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SteveTheTradeBot.Api.Mappers;
using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Dal.Models.Users;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Serilog;

namespace SteveTheTradeBot.Api.Security
{
    public class PersistedGrantStore : IPersistedGrantStore
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUserGrantLookup _userGrantLookup;
        private readonly IUserLookup _userLookup;

        #region Implementation of IPersistedGrantStore

        public PersistedGrantStore(IUserGrantLookup userGrantLookup, IUserLookup userLookup)
        {
            _userGrantLookup = userGrantLookup;
            _userLookup = userLookup;
        }

        public async Task StoreAsync(PersistedGrant grant)
        {
            _log.Information($"PersistedGrantStore:StoreAsync store sessions for SubjectId '{grant.SubjectId}' ");
            var userGrant = grant.ToGrant();
            var userById = await _userLookup.GetById(grant.SubjectId);
            if (userById != null) userGrant.User = userById.ToReference();
            await _userGrantLookup.Insert(userGrant);
        }

        public async Task<PersistedGrant> GetAsync(string key)
        {
            var byKey = await _userGrantLookup.GetByKey(key);
            return byKey.ToPersistanceGrant();
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            var fromDbByFilter = await FromDbByFilter(filter);
            return fromDbByFilter.Select(x => x.ToPersistanceGrant());
        }

        public async Task RemoveAsync(string key)
        {
            var byKey = await _userGrantLookup.GetByKey(key);
            if (byKey != null) await _userGrantLookup.Delete(byKey.Id);
        }

        public async Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            _log.Warning($"PersistedGrantStore:RemoveAllAsync For client {filter.SubjectId} {filter.ClientId} ");
            var fromDbByFilter = await FromDbByFilter(filter);
            foreach (var userGrant in fromDbByFilter)
                await _userGrantLookup.Delete(userGrant.Id);
        }

        private async Task<IEnumerable<UserGrant>> FromDbByFilter(PersistedGrantFilter filter)
        {
            _log.Information($"PersistedGrantStore:FromDbByFilter For SubjectId `{filter.SubjectId}` ClientId `{filter.ClientId}` ");
            return (await _userGrantLookup.GetByUserId(filter.SubjectId)).Where(x => x.ClientId == filter.ClientId);
        }

        #endregion
    }
}