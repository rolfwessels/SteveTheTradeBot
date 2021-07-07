using System.Collections.Generic;
using AutoMapper;
using SteveTheTradeBot.Dal.Models.Users;
using SteveTheTradeBot.Shared.Models.Users;

namespace SteveTheTradeBot.Api.Mappers
{
    public static partial class MapApi
    {


        public static RoleModel ToModel(this Role user, RoleModel model = null)
        {
            return GetInstance().Map(user, model);
        }


        #region Private Methods

        private static void MapUserModel(IMapperConfigurationExpression configuration)
        {
            
            configuration.CreateMap<Role, RoleModel>();
            configuration.CreateMap<UserCreateUpdateModel, User>()
                .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email.ToLower()))
                .ForMember(x => x.LastLoginDate, opt => opt.Ignore())
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.Roles, opt => opt.Ignore())
                .ForMember(x => x.HashedPassword, opt => opt.Ignore())
                .ForMember(x => x.DefaultProject, opt => opt.Ignore())
                .ForMember(x => x.CreateDate, opt => opt.Ignore())
                .ForMember(x => x.UpdateDate, opt => opt.Ignore());

            configuration.CreateMap<RegisterModel, User>()
                .ForMember(x => x.Email, opt => opt.MapFrom(x => x.Email.ToLower()))
                .ForMember(x => x.LastLoginDate, opt => opt.Ignore())
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.Roles, opt => opt.Ignore())
                .ForMember(x => x.LastLoginDate, opt => opt.Ignore())
                .ForMember(x => x.HashedPassword, opt => opt.Ignore())
                .ForMember(x => x.DefaultProject, opt => opt.Ignore())
                .ForMember(x => x.CreateDate, opt => opt.Ignore())
                .ForMember(x => x.UpdateDate, opt => opt.Ignore());
        }

        #endregion
    }
}