using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace SteveTheTradeBot.Core.Framework.CommandQuery
{
    public class MediatorCommander : ICommander
    {
        private readonly IMediator _mediator;

        public MediatorCommander(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Implementation of ICommander

        public async Task Notify<T>(T notificationRequest, CancellationToken cancellationToken) where T : CommandNotificationBase
        {
            await _mediator.Publish(notificationRequest);
        }

        public async Task<CommandResult> Execute<T>(T commandRequest, CancellationToken cancellationToken) where T : CommandRequestBase
        {
            return await _mediator.Send(commandRequest);
        }

        #endregion
    }
}