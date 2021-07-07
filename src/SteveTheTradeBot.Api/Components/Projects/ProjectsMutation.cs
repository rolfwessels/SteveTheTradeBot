using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.Projects;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Dal.Persistence;
using SteveTheTradeBot.Shared.Models.Projects;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

namespace SteveTheTradeBot.Api.Components.Projects
{
    public class ProjectsMutation
    {
        private readonly ICommander _commander;
        private readonly IIdGenerator _generator;

        public ProjectsMutation(ICommander commander, IIdGenerator generator)
        {
            _commander = commander;
            _generator = generator;
        }

        public Task<CommandResult> Create(
            [GraphQLNonNullType]
            [GraphQLType(typeof(NonNullType<ProjectCreateUpdateType>))]
            ProjectCreateUpdateModel project)
        {
            return _commander.Execute(ProjectCreate.Request.From(_generator.NewId, project.Name), CancellationToken.None);
        }

        [Authorize]
        public Task<CommandResult> Update(
            [GraphQLNonNullType] string id,
            [GraphQLType(typeof(NonNullType<ProjectCreateUpdateType>))]
            ProjectCreateUpdateModel project)
        {
            return _commander.Execute(ProjectUpdateName.Request.From(id, project.Name), CancellationToken.None);
        }

        public Task<CommandResult> Remove([GraphQLNonNullType] string id)
        {
            return _commander.Execute(ProjectRemove.Request.From(id), CancellationToken.None);
        }
    }
}