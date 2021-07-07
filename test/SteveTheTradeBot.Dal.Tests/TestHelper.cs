using System;
using System.Threading;
using System.Threading.Tasks;
using Moq.Language.Flow;

namespace SteveTheTradeBot.Dal.Tests
{
    public static class TestHelper
    {
        public static void Returns<T1, T2>(this ISetup<T1, Task<T2>> setup, T2 dal) where T1 : class
        {
            setup.Returns(Task.FromResult(dal));
        }

        public static T WaitForValue1<T>(ref T dff)
        {
            throw new NotImplementedException();
        }

        public static T WaitForValue<T>(Func<T> func, int timeOut = 500)
        {
            var waitForValue = func();
            var expire = DateTime.Now.AddMilliseconds(timeOut);
            while (waitForValue == null && DateTime.Now < expire)
            {
                Thread.Sleep(100);
                waitForValue = func();
                
            }

            return waitForValue;
        }
    }
}