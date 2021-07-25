using System;

namespace SteveTheTradeBot.Core.Utils
{
    public static class DateTimeHelper
    {
        public static string ToIsoDateString(this DateTime toUniversalTime)
        {
            return toUniversalTime.ToString("o");
        }

        public static string ToShort(this TimeSpan stopwatchElapsed, int decimals = 1)
        {
            if (stopwatchElapsed.TotalDays > 30) return $"{Math.Round(stopwatchElapsed.TotalDays/30, decimals)}m";
            if (stopwatchElapsed.TotalDays > 1) return $"{Math.Round(stopwatchElapsed.TotalDays, decimals)}d";
            if (stopwatchElapsed.TotalHours > 1) return $"{Math.Round(stopwatchElapsed.TotalHours, decimals)}h";
            if (stopwatchElapsed.TotalMinutes > 1) return $"{Math.Round(stopwatchElapsed.TotalMinutes, decimals)}m";
            if (stopwatchElapsed.TotalSeconds > 1) return $"{Math.Round(stopwatchElapsed.TotalSeconds, decimals)}s";
            if (stopwatchElapsed.TotalMilliseconds > 1) return $"{Math.Round(stopwatchElapsed.TotalMilliseconds,decimals)}ms";
            return "0ms";
        }

        public static DateTime ToMinute(this DateTime now)
        {
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
        }

        public static DateTime To30Seconds(this DateTime now)
        {
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second >= 30? 30:0, now.Kind);
        }

        public static TimeSpan TimeTill(this DateTime toMinute)
        {
            return toMinute - DateTime.Now;
        }
    }
}