using System;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Dal.Persistence;

namespace SteveTheTradeBot.Core.Components.Users
{
    public class UserRemove
    {
        #region Nested type: Handler

        public class Handler : CommandHandlerBase<Request>
        {
            private readonly ICommander _commander;
            private readonly IGeneralUnitOfWorkFactory _persistence;

            public Handler(IGeneralUnitOfWorkFactory persistence,
                ICommander commander)
            {
                _persistence = persistence;
                _commander = commander;
            }

            #region Overrides of CommandHandlerBase<Request>

            public override async Task ProcessCommand(Request request, CancellationToken cancellationToken)
            {
                using (var connection = _persistence.GetConnection())
                {
                    var foundUser = await connection.Users.FindOrThrow(request.Id);
                    var removed = await connection.Users.Remove(x => x.Id == foundUser.Id);
                    await _commander.Notify(request.ToEvent(removed), cancellationToken);
                }
            }

            #endregion
        }

        #endregion

        #region Nested type: Notification

        public class Notification : CommandNotificationBase
        {
            public bool WasRemoved { get; set; }

            #region Overrides of CommandNotificationBase

            public override string EventName => "UserRemoved";

            #endregion
        }

        #endregion

        #region Nested type: Request

        public class Request : CommandRequestBase
        {
            public static Request From(string id)
            {
                if (id == null) throw new ArgumentNullException(nameof(id));

                return new Request
                {
                    Id = id,
                };
            }
        }

        #endregion
    }
}