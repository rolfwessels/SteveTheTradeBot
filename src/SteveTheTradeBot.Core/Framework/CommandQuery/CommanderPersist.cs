using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Serializer;
using SteveTheTradeBot.Core.Framework.Event;
using SteveTheTradeBot.Dal.Models.SystemEvents;
using SteveTheTradeBot.Dal.Persistence;


namespace SteveTheTradeBot.Core.Framework.CommandQuery
{
    public class CommanderPersist : ICommander
    {
        private readonly ICommander _commander;
        private readonly IRepository<SystemCommand> _repository;
        private readonly IStringify _stringify;
        private readonly IEventStoreConnection _eventStore;

        public CommanderPersist(ICommander commander, IRepository<SystemCommand> repository, IStringify stringify, IEventStoreConnection eventStore)
        {
            _commander = commander;
            _repository = repository;
            _stringify = stringify;
            _eventStore = eventStore;
        }

        #region Implementation of ICommander

        public async Task Notify<T>(T notificationRequest, CancellationToken cancellationToken) where T : CommandNotificationBase
        {
            await _eventStore.Append(notificationRequest, cancellationToken);
            await _commander.Notify(notificationRequest, cancellationToken);
        }

        public async Task<CommandResult> Execute<T>(T commandRequest, CancellationToken cancellationToken) where T : CommandRequestBase
        {
            var commandResult = await _commander.Execute(commandRequest, cancellationToken);
            await _repository.Add(new SystemCommand(
                commandRequest.CorrelationId,
                commandRequest.CreatedAt,
                commandRequest.Id,
                SystemCommand.BuildTypeName(commandRequest),
                _stringify.Serialize(commandRequest)
            ));
            
            return commandResult;

        }

        #endregion
    }
}