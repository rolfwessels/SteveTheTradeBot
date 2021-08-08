using System;

namespace SteveTheTradeBot.Core.Components.SlackResponders
{
    public static class SlackHelper
    {
        public static string GetGreeting()
        {
            if (DateTime.Now.Hour <= 12)
            {
                return "Good Morning";
            }

            if (DateTime.Now.Hour <= 16)
            {
                return ("Good Afternoon");
            }
            if (DateTime.Now.Hour <= 20)
            {
                return ("Good Evening");
            }
            
            {
                return ("Good Night");
            }
        }
    }
}