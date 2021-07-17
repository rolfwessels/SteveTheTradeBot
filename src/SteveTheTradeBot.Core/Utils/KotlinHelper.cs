using System;

namespace SteveTheTradeBot.Core.Utils
{
    public static class KotlinHelper    
    {
        public static T With<T>(this T value, Action<T> action)
        {
            action(value);
            return value;
        }
    }
}