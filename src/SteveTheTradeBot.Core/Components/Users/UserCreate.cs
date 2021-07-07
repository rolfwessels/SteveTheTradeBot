using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Dal.Persistence;
using SteveTheTradeBot.Dal.Validation;

namespace SteveTheTradeBot.Core.Components.Users
{
    public class UserCreate
    {
        #region Nested type: Handler

        public class Handler : CommandHandlerBase<Request>
        {
            private readonly ICommander _commander;
            private readonly IValidatorFactory _validation;
            private readonly IGeneralUnitOfWorkFactory _persistance;

            public Handler(IGeneralUnitOfWorkFactory persistance, IValidatorFactory validation,
                ICommander commander)
            {
                _persistance = persistance;
                _validation = validation;
                _commander = commander;
            }

            #region Overrides of CommandHandlerBase<Request>

            public override async Task ProcessCommand(Request request, CancellationToken cancellationToken)
            {
                var user = request.ToDao();
                using (var connection = _persistance.GetConnection())
                {
                    _validation.ValidateAndThrow(user);
                    user.ValidateRolesAndThrow();
                    await connection.Users.Add(user);
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
            public List<string> Roles { get; set; }

            #region Overrides of CommandNotificationBase

            public override string EventName => "UserCreated";

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

            public static Request From(string id, string name, string email, string password, List<string> roles)
            {
                if (id == null) throw new ArgumentNullException(nameof(id));
                if (name == null) throw new ArgumentNullException(nameof(name));
                if (email == null) throw new ArgumentNullException(nameof(email));
                if (roles == null || !roles.Any()) throw new ArgumentNullException(nameof(roles));

                return new Request
                {
                    Id = id,
                    Name = name,
                    Email = email,
                    Password = password,
                    Roles = roles
                };
            }
        }

        #endregion
    }
}