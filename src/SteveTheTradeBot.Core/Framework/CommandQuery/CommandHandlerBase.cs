using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.Mappers;
using Serilog;
using MediatR;
using Serilog.Context;

namespace SteveTheTradeBot.Core.Framework.CommandQuery
{
    public abstract class CommandHandlerBase<T> : IRequestHandler<T, CommandResult> where T : CommandRequestBase
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        #region IRequestHandler<T,CommandResult> Members

        public async Task<CommandResult> Handle(T request, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Topic", request))
            {
                try
                {
                    await ProcessCommand(request, cancellationToken);
                    return request.ToCommandResult();
                }
                catch (Exception e)
                {
                    _log.Error($"CommandHandlerBase:Handle {e.Message}");
                    throw;
                }
            }
        }

        #endregion

        public abstract Task ProcessCommand(T request, CancellationToken cancellationToken);
    }
}