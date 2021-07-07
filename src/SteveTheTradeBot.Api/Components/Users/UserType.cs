using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Dal.Models.Users;
using HotChocolate.Types;

namespace SteveTheTradeBot.Api.Components.Users
{
    public class UserType : ObjectType<User>
    {
        protected override void Configure(IObjectTypeDescriptor<User> descriptor)
        {
            Name = "User";
            descriptor.Field(d => d.Id).Type<NonNullType<StringType>>().Description("The id of the user.");
            descriptor.Field(d => d.Name).Type<NonNullType<StringType>>().Description("The name of the user.");
            descriptor.Field(d => d.Email).Type<NonNullType<StringType>>().Description("The value of the user.");
            descriptor.Field(d => d.Roles)
                .Type<NonNullType<ListType<NonNullType<StringType>>>>().Description("The roles of the user.");
            descriptor.Field("image")
                .Type<NonNullType<StringType>>()
                .Resolver(context => { return GravatarHelper.BuildUrl(context.Parent<User>().Email); })
                .Description("User profile image.");
            descriptor.Field("activities")
                .Type<NonNullType<ListType<NonNullType<StringType>>>>()
                .Resolver(context => Roles(context.Parent<User>()?.Roles))
                .Description("The activities that this user is authorized for.");
            descriptor.Field(d => d.UpdateDate).Type<NonNullType<DateTimeType>>()
                .Description("The date when the user was last updated.");
            descriptor.Field(d => d.CreateDate).Type<NonNullType<DateTimeType>>()
                .Description("The date when the user was created.");
        }


        #region Private Methods

        private static List<string> Roles(List<string> sourceRoles)
        {
            var roles = sourceRoles.Select(RoleManager.GetRole)
                .Where(x => x != null)
                .SelectMany(x => x.Activities)
                .Distinct()
                .Select(x => x.ToString())
                .OrderBy(x => x)
                .ToList();
            return roles;
        }

        #endregion
    }
}