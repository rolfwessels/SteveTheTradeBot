using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using SteveTheTradeBot.Core;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.Settings;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Cmd
{
    public class UtilCommand
    {
        public class Encrypt : AsyncCommandWithToken<Encrypt.Settings>
        {
            public sealed class Settings : BaseCommandSettings
            {
                [CommandOption("--enc")]
                [Description("Value to encrypt")]
                public string Enc { get; set; }
            }

            #region Overrides of CommandSync<BaseCommandSettings>

            public override Task ExecuteAsync(Encrypt.Settings settings, CancellationToken token)
            {
               
                var configurationRoot = new ConfigurationBuilder().AddJsonFilesAndEnvironment().Build();
                var sampleSettings = new SampleSettings(configurationRoot);


                var encryptionKey = sampleSettings.Key;
                var environmentVariable = ConfigurationBuilderHelper.GetEnvironment();
                AnsiConsole.MarkupLine($"Encrypting '{settings.Enc}' for env:[yellow]{environmentVariable}[/] key:[yellow]{encryptionKey?.Substring(0, 3)}xxxxxxxxxxxxxx[/]");

                Console.Out.WriteLine(sampleSettings.EncryptString(settings.Enc));
                return Task.CompletedTask;
            }

            #endregion

            public class SampleSettings : BaseEncryptedSettings
            {
                public SampleSettings(IConfiguration configuration) : base(configuration, "")
                {
                    
                }

                public string Key => ReadEncryptedValue("EncryptionKey", "uhhh");

            }
        }

        public class Environment : AsyncCommandWithToken<BaseCommandSettings>
        {
           

            #region Overrides of CommandSync<BaseCommandSettings>

            public override Task ExecuteAsync(BaseCommandSettings settings, CancellationToken token)
            {
                AnsiConsole.MarkupLine($"[grey]Environment [/][green]{ ConfigurationBuilderHelper.GetEnvironment() }[/]");
                AnsiConsole.MarkupLine($"[grey]MongoConnection [/][white]{ Settings.Instance.MongoConnection }[/]");
                AnsiConsole.MarkupLine($"[grey]NpgsqlConnection [/][white]{ Settings.Instance.NpgsqlConnection }[/]");
                AnsiConsole.MarkupLine($"[grey]RedisHost [/][white]{ Settings.Instance.RedisHost }[/]");
                AnsiConsole.MarkupLine($"[grey]SlackBotKey [/][white]{ Settings.Instance.SlackBotKey }[/]");
                AnsiConsole.MarkupLine($"[grey]SlackChannel [/][white]{ Settings.Instance.SlackChannel }[/]");
                AnsiConsole.MarkupLine($"[grey]LogsSlackChannel [/][white]{ Settings.Instance.LogsSlackChannel }[/]");
                AnsiConsole.MarkupLine($"[grey]SlackWebhookUrl [/][white]{ Settings.Instance.SlackWebhookUrl }[/]");
                AnsiConsole.MarkupLine($"[grey]LogFolder [/][white]{ Settings.Instance.LogFolder }[/]");
                AnsiConsole.MarkupLine($"[grey]DataProtectionFolder [/][white]{ Settings.Instance.DataProtectionFolder }[/]");
                AnsiConsole.MarkupLine($"[grey]LokiUrl [/][white]{ Settings.Instance.LokiUrl }[/]");
                AnsiConsole.MarkupLine($"[grey]LokiUser [/][white]{ Settings.Instance.LokiUser }[/]");
                AnsiConsole.MarkupLine($"[grey]LokiPassword [/][white]{ Settings.Instance.LokiPassword }[/]");
                AnsiConsole.MarkupLine($"[grey]Valr ApiKey [/][white]{ ValrSettings.Instance.ApiKey }[/]");
                AnsiConsole.MarkupLine($"[grey]Valr Secret [/][white]{ ValrSettings.Instance.Secret }[/]");
                return Task.CompletedTask;
            }

            #endregion

            public class SampleSettings : BaseEncryptedSettings
            {
                public SampleSettings(IConfiguration configuration) : base(configuration, "")
                {

                }

                public string Key => ReadEncryptedValue("EncryptionKey", "uhhh");

            }
        }
    }

   
}