using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
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
                [Description("Value to encrypt.")]
                public string Enc { get; set; }
            }

            #region Overrides of CommandSync<BaseCommandSettings>

            public override Task ExecuteAsync(Encrypt.Settings settings, CancellationToken token)
            {
               
                var configurationRoot = new ConfigurationBuilder().AddJsonFilesAndEnvironment().Build();
                var sampleSettings = new SampleSettings(configurationRoot);


                var encryptionKey = sampleSettings.Key;
                var environmentVariable = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                AnsiConsole.MarkupLine($"Encrypting '{settings.Enc}' for env:[yellow]{environmentVariable}[/] key:[yellow]{encryptionKey?.Substring(0, 3)}xxxxxxxxxxxxxx[/].");

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
    }

   
}