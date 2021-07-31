using System.Threading;
using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace SteveTheTradeBot.Cmd
{
    public abstract class CommandSync<T> : Command<T> where T : CommandSettings
    {
        #region Overrides of Command<T>

        public override int Execute(CommandContext context, T settings)
        {
            var bindToCancelKey = ConsoleHelper.BindToCancelKey();
            // ReSharper disable once MethodSupportsCancellation
            ExecuteAsync(settings, bindToCancelKey.Token).Wait();
            return 0;

        }

        public abstract Task ExecuteAsync(T settings, CancellationToken token);

        #endregion
    }
}