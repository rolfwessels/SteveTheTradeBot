using SteveTheTradeBot.Api.Components.Projects;
using SteveTheTradeBot.Api.Components.Users;
using HotChocolate.Types;

namespace SteveTheTradeBot.Api.GraphQl
{
    public class DefaultQuery : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            Name = "Query";
            descriptor.Field("projects")
                .Type<NonNullType<ProjectsQueryType>>()
                .Resolver(x => new ProjectsQueryType.ProjectQuery());
            descriptor.Field("users")
                .Type<NonNullType<UsersQueryType>>()
                .Resolver(x => new UsersQueryType.UsersQuery());
        }
    }
}