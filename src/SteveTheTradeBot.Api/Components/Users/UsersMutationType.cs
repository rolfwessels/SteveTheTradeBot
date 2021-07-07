using System.Reflection;
using SteveTheTradeBot.Api.GraphQl;
using SteveTheTradeBot.Dal.Models.Auth;
using SteveTheTradeBot.Shared.Models.Users;
using HotChocolate.Types;
using Serilog;

namespace SteveTheTradeBot.Api.Components.Users
{
    public class UsersMutationType : ObjectType<UsersMutation>
    {
        protected override void Configure(IObjectTypeDescriptor<UsersMutation> descriptor)
        {
            Name = "UsersMutation";

            descriptor.Field(x => x.Create(default(UserCreateUpdateModel)))
                .Description("Add a user.")
                .RequirePermission(Activity.UpdateUsers);

            descriptor.Field(x => x.Update(default(string), default(UserCreateUpdateModel)))
                .Description("Update a user.")
                .RequirePermission(Activity.UpdateUsers);

            descriptor.Field(x => x.Remove(default(string)))
                .Description("Permanently remove a user.")
                .RequirePermission(Activity.DeleteUser);

            descriptor.Field(x => x.Register(default(RegisterModel)))
                .Description("Register a new user.");
        }
    }
}