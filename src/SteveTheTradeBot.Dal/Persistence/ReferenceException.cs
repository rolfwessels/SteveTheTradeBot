using System;

namespace SteveTheTradeBot.Dal.Persistence
{
    public class ReferenceException : Exception
    {
        public ReferenceException(string message) : base(message)
        {
        }
    }
}