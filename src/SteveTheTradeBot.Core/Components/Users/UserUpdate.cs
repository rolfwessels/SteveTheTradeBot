using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Dal.Persistence;
using SteveTheTradeBot.Dal.Validation;

namespace SteveTheTradeBot.Core.Components.Users
{
    public class UserUpdate
    {
        #region Nested type: Handler

        public class Handler : CommandHandlerBase<Request>
        {
            private readonly ICommander _commander;
            private readonly IValidatorFactory _validation;
            private readonly IGeneralUnitOfWorkFactory _persistence;

            public Handler(IGeneralUnitOfWorkFactory persistence, IValidatorFactory validation,
                ICommander commander)
            {
                _persistence = persistence;
                _validation = validation;
                _commander = commander;
            }

            #region Overrides of CommandHandlerBase<Request>

            public override async Task ProcessCommand(Request request, CancellationToken cancellationToken)
            {
                using (var connection = _persistence.GetConnection())
                {
                    var foundUser = await connection.Users.FindOrThrow(request.Id);
                    var updatedUser = request.ToDao(foundUser);


                    _validation.ValidateAndThrow(updatedUser);
                    await connection.Users.UpdateOne(x => x.Id == foundUser.Id, calls => calls
                        .Set(x => x.Name, updatedUser.Name)
                        .Set(x => x.Email, updatedUser.Email)
                        .Set(x => x.HashedPassword, updatedUser.HashedPassword)
                        .Set(x => x.Roles, updatedUser.Roles)
                    );
                }

                await _commander.Notify(request.ToEvent(), cancellationToken);
            }

            #endregion
        }

        #endregion

        #region Nested type: Notification

        public class Notification : CommandNotificationBase
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public bool PasswordChanged { get; set; }
            public List<string> Roles { get; set; }

            #region Overrides of CommandNotificationBase

            public override string EventName => "UserUpdated";

            #endregion
        }

        #endregion

        #region Nested type: Request

        public class Request : CommandRequestBase
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public List<string> Roles { get; set; }

            public static Request From(string id, string name, string password, List<string> roles, string email)
            {
                if (id == null) throw new ArgumentNullException(nameof(id));
                if (name == null) throw new ArgumentNullException(nameof(name));
                if (email == null) throw new ArgumentNullException(nameof(email));
//                if (roles == null || !roles.Any()) throw new ArgumentNullException(nameof(roles));

                return new Request
                {
                    Id = id,
                    Name = name,
                    Password = password,
                    Email = email,
                    Roles = roles
                };
            }
        }

        #endregion
    }
}