using System.Threading.Tasks;
using SteveTheTradeBot.Api.Components.Projects;
using SteveTheTradeBot.Api.Components.Users;
using HotChocolate.Types;

namespace SteveTheTradeBot.Api.GraphQl
{
    public class DefaultMutation : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            Name = "Mutation";
            descriptor.Field("projects")
                .Type<NonNullType<ProjectsMutationType>>()
                .Resolver(x => x.Resolver<ProjectsMutation>());
            descriptor.Field("users")
                .Type<NonNullType<UsersMutationType>>()
                .Resolver(x => x.Resolver<UsersMutation>());
        }
    }
}