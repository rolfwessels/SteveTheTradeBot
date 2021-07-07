using System;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Dal.Persistence;
using SteveTheTradeBot.Dal.Validation;

namespace SteveTheTradeBot.Core.Components.Projects
{
    public class ProjectCreate
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
                var project = request.ToDao();
                using (var connection = _persistence.GetConnection())
                {
                    _validation.ValidateAndThrow(project);
                    await connection.Projects.Add(project);
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

            #region Overrides of CommandNotificationBase

            public override string EventName => "ProjectCreated";

            #endregion
        }

        #endregion

        #region Nested type: Request

        public class Request : CommandRequestBase
        {
            public string Name { get; set; }

            public static Request From(string id, string name)
            {
                if (id == null) throw new ArgumentNullException(nameof(id));
                if (name == null) throw new ArgumentNullException(nameof(name));

                return new Request
                {
                    Id = id,
                    Name = name
                };
            }
        }

        #endregion
    }
}