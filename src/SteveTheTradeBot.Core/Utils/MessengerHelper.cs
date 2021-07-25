using System;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.MessageUtil;

namespace SteveTheTradeBot.Core.Utils
{
    public class MessengerHelper
    {
        public static void RegisterAsync<T>(IMessenger messenger, object slackAlertService, Func<T,Task> action) where T : class
        {
            messenger.Register<T>(slackAlertService,(r) => action(r).With(x=>x.ConfigureAwait(false)).Wait());
        }
    }
}