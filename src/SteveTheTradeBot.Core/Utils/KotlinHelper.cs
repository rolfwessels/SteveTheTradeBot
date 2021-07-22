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

        public static T2 Let<T,T2>(this T rsi, Func<T,T2> let) 
        {
            return let(rsi);
        }
    }
}