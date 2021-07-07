using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Dal.Models.Auth;
using HotChocolate.Types;

namespace SteveTheTradeBot.Api.GraphQl
{
    public static class GraphQlExtensions
    {
        public static void RequirePermission(this IObjectFieldDescriptor type, Activity permission)
        {
            type.Authorize(RoleManager.GetRoles(permission));
        }

        public static void RequireAuthorization(this IObjectFieldDescriptor type)
        {
            type.Authorize();
        }
    }
}