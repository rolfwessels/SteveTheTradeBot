using System;
using Serilog;

namespace SteveTheTradeBot.Core.Framework.Logging
{
    public static class LoggingHelper
    {
        private static bool _hasValue;
        private static readonly object _locker = new object();

        public static ILogger SetupOnce(Func<ILogger> func)
        {
            if (!_hasValue)
                lock (_locker)
                {
                    if (!_hasValue)
                    {
                        _hasValue = true;
                        return Log.Logger = func();
                    }
                }

            return Log.Logger;
        }

        public const int MB = 1048576;
    }
}