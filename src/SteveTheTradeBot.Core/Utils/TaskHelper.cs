using System;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;

namespace SteveTheTradeBot.Core.Utils
{
    public static class TaskHelper
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        public static void OnSuccess<TType>(
            this Task<TType> task,
            Action<TType> continueWith)
        {
            task
                .With(x=>x.ConfigureAwait(false))
                .ContinueWith((t) =>
                {
                    if (t.Exception == null) continueWith(t.Result);
                    else
                    {
                        _log.Error(t.Exception,t.Exception.Message);
                    } 
                });
        }

    }
}