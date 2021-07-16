using System;

namespace SteveTheTradeBot.Core.Utils
{
    public class Gu
    {
        public static string Id()
        {
            return Guid.NewGuid().ToString("n");
        }
    }
}