using AutoMapper;
using SteveTheTradeBot.Dal.Models.Users;
using IdentityServer4.Models;

namespace SteveTheTradeBot.Api.Mappers
{
    public static partial class MapApi
    {
        public static UserGrant ToGrant(this PersistedGrant model, UserGrant userGrant = null)
        {
            return GetInstance().Map(model, userGrant);
        }

        public static PersistedGrant ToPersistanceGrant(this UserGrant userGrant, PersistedGrant model = null)
        {
            return GetInstance().Map(userGrant, model);
        }

        #region Private Methods

        private static void MapUserGrantModel(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<PersistedGrant, UserGrant>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.User, opt => opt.MapFrom(x => new UserReference {Id = x.SubjectId}))
                .ForMember(x => x.CreateDate, opt => opt.MapFrom(x => x.CreationTime))
                .ForMember(x => x.UpdateDate, opt => opt.Ignore());
            configuration.CreateMap<UserGrant, PersistedGrant>()
                .ForMember(x => x.CreationTime, opt => opt.MapFrom(x => x.CreateDate))
                .ForMember(x => x.Description, opt => opt.Ignore())
                .ForMember(x => x.SessionId, opt => opt.Ignore())
                .ForMember(x => x.ConsumedTime, opt => opt.Ignore())
                .ForMember(x => x.SubjectId, opt => opt.MapFrom(x => x.User.Id));
        }

        #endregion
    }
}