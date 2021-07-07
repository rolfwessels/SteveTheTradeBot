using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Moq.Language.Flow;

namespace SteveTheTradeBot.Dal.Tests
{
    public static class TestHelper
    {
        public static void Returns<T1, T2>(this ISetup<T1, Task<T2>> setup, T2 dal) where T1 : class
        {
            setup.Returns(Task.FromResult(dal));
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

        public static async Task TestEveryNowAndThen(Func<Task> action, [CallerMemberName] string caller = null)
        {
            var file = Path.Combine(Path.GetTempPath(), $"zzz_{caller}.txt");
            if (!File.Exists(file) || DateTime.Parse(File.ReadAllText(file)) < DateTime.Now)
            {
                await action();
                File.WriteAllText(file,DateTime.Now.AddMinutes(1).ToString("o"));
            }
            else
            {
                Console.Out.WriteLine($"TestEveryNowAndThen: Skipped {caller}");
            }
        }
    }
}