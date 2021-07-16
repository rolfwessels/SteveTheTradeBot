using System;

namespace SteveTheTradeBot.Core.Utils
{
    public static class DateTimeHelper
    {
        public static string ToIsoDateString(this DateTime toUniversalTime)
        {
            return toUniversalTime.ToString("o");
        }

        public static string ToShort(this TimeSpan stopwatchElapsed)
        {
            if (stopwatchElapsed.TotalHours > 1) return $"{Math.Round(stopwatchElapsed.TotalHours)}h";
            if (stopwatchElapsed.TotalMinutes > 1) return $"{Math.Round(stopwatchElapsed.TotalMinutes)}m";
            if (stopwatchElapsed.TotalSeconds > 1) return $"{Math.Round(stopwatchElapsed.TotalSeconds)}s";
            if (stopwatchElapsed.TotalMilliseconds > 1) return $"{Math.Round(stopwatchElapsed.TotalMilliseconds)}ms";
            return "0ms";
        }
    }
}