using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Api.Components.Projects;
using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Dal.Persistence;
using SteveTheTradeBot.Shared.Models.Users;
using HotChocolate;
using HotChocolate.Types;

namespace SteveTheTradeBot.Api.Components.Users
{
    public class UsersMutation
    {
        private readonly ICommander _commander;
        private readonly IIdGenerator _generator;

        public UsersMutation(ICommander commander, IIdGenerator generator)
        {
            _commander = commander;
            _generator = generator;
        }

        public Task<CommandResult> Create(
            [GraphQLType(typeof(NonNullType<UserCreateUpdateType>))]
            UserCreateUpdateModel user)
        {
            return _commander.Execute(UserCreate.Request.From(_generator.NewId, user.Name, user.Email,
                user.Password, user.Roles), CancellationToken.None);
        }

        public Task<CommandResult> Update(
            [GraphQLNonNullType] string id,
            [GraphQLType(typeof(NonNullType<UserCreateUpdateType>))]
            UserCreateUpdateModel user)
        {
            return _commander.Execute(UserUpdate.Request.From(id, user.Name, user.Password, user.Roles,
                user.Email), CancellationToken.None);
        }

        public Task<CommandResult> Remove([GraphQLNonNullType] string id)
        {
            return _commander.Execute(UserRemove.Request.From(id), CancellationToken.None);
        }

        public Task<CommandResult> Register([GraphQLNonNullType] [GraphQLType(typeof(RegisterType))]
            RegisterModel user)
        {
            return _commander.Execute(UserCreate.Request.From(_generator.NewId, user.Name, user.Email,
                user.Password, new List<string>() {RoleManager.Guest.Name}), CancellationToken.None);
        }
    }
}