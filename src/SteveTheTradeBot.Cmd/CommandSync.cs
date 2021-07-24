using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace SteveTheTradeBot.Cmd
{
    public abstract class CommandSync<T> : Command<T> where T : CommandSettings
    {
        #region Overrides of Command<T>

        public override int Execute(CommandContext context, T settings)
        {
            ExecuteAsync(settings).Wait();
            return 0;

        }

        public abstract Task ExecuteAsync(T settings);

        #endregion
    }
}