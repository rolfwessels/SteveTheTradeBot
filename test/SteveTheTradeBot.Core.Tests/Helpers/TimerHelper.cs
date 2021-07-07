using System;

namespace SteveTheTradeBot.Core.Tests.Helpers
{
    public static class TimerHelper
    {
        public static T WaitFor<T>(this T updateModels, Func<T, bool> o, int timeOut = 500)
        {
            var stopTime = DateTime.Now.AddMilliseconds(timeOut);
            bool result;
            do
            {
                result = o(updateModels);
            } while (!result && stopTime > DateTime.Now);

            return updateModels;
        }
    }
}