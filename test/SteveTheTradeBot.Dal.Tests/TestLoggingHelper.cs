using System;
using SteveTheTradeBot.Core.Framework.Logging;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace SteveTheTradeBot.Dal.Tests
{
    [SetUpFixture]
    public class TestLoggingHelper
    {
        private static Lazy<ILogger> _logger;

        static TestLoggingHelper()
        {
            _logger = new Lazy<ILogger>(SetupOnce);
        }

        public static void EnsureExists()
        {
            Log.Logger = _logger.Value;
        }

        private static ILogger SetupOnce()
        {
            return LoggingHelper.SetupOnce(() => new LoggerConfiguration().MinimumLevel.Debug()
                .WriteTo.Console(LogEventLevel.Debug)
                .CreateLogger());
        }
    }
}