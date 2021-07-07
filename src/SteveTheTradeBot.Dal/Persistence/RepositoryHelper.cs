using System;
using System.Threading.Tasks;
using SteveTheTradeBot.Dal.Models.Base;
using Bumbershoot.Utilities.Helpers;

namespace SteveTheTradeBot.Dal.Persistence
{
    public static class RepositoryHelper
    {
        public static async Task<T> FindOrThrow<T>(this IRepository<T> repo, string requestId)
            where T : IBaseDalModelWithId
        {
            var foundUser = await repo.FindOne(x => x.Id == requestId);

            return foundUser.ExistsOrThrow(requestId);
        }

        public static T ExistsOrThrow<T>(this T found, string requestId) where T : IBaseDalModelWithId
        {
            if (found == null)
                throw new ReferenceException(
                    $"Invalid {typeof(T).Name.UnderScoreAndCamelCaseToHumanReadable()} id [{requestId}].");
            return found;
        }
    }
}