using System.ComponentModel;
using Spectre.Console.Cli;

namespace SteveTheTradeBot.Cmd
{
  public class BaseCommandSettings : CommandSettings
  {
    [CommandOption("-v")]
    [Description("Verbose")]
    public bool Verbose { get; set; }
  }
}
