using System;

namespace SteveTheTradeBot.Core.Utils
{
    public static class DateTimeHelper
    {
        public static string ToIsoDateString(this DateTime toUniversalTime)
        {
            return toUniversalTime.ToString("o");
        }
    }
}