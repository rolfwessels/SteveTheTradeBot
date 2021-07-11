using System;
using Spectre.Console.Cli;

namespace SteveTheTradeBot.Cmd
{
    public class DataImportCommand : Command<DataImportCommand.Settings>
    {
        public sealed class Settings : BaseCommandSettings
        {
           
        }

        #region Overrides of Command<Settings>

        public override int Execute(CommandContext context, Settings settings)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}