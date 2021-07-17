﻿using System;

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
            if (stopwatchElapsed.TotalHours > 1) return $"{Math.Round(stopwatchElapsed.TotalHours, decimals)}h";
            if (stopwatchElapsed.TotalMinutes > 1) return $"{Math.Round(stopwatchElapsed.TotalMinutes, decimals)}m";
            if (stopwatchElapsed.TotalSeconds > 1) return $"{Math.Round(stopwatchElapsed.TotalSeconds, decimals)}s";
            if (stopwatchElapsed.TotalMilliseconds > 1) return $"{Math.Round(stopwatchElapsed.TotalMilliseconds,decimals)}ms";
            return "0ms";
        }
    }
}