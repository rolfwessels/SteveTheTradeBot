using System.Threading;
using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace SteveTheTradeBot.Cmd
{
    public abstract class AsyncCommandWithToken<T> : AsyncCommand<T> where T : CommandSettings
    {
        #region Overrides of Command<T>

        #region Overrides of AsyncCommand<T>

        public override async Task<int> ExecuteAsync(CommandContext context, T settings)
        {
            var bindToCancelKey = ConsoleHelper.BindToCancelKey();
            await ExecuteAsync(settings, bindToCancelKey.Token);
            return 0;
        }

        #endregion


        public abstract Task ExecuteAsync(T settings, CancellationToken token);

        #endregion
    }
}