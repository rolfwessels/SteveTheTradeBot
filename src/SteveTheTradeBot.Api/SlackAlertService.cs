using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core;
using SteveTheTradeBot.Core.Components.Notifications;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Framework.Slack;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Api
{
    public class SlackAlertService : BackgroundService
    {
        private readonly IMessenger _messenger;
        private readonly MessageToNotification _messageToNotification;
        private readonly INotificationChannel _notificationChannel;
        private readonly SlackService _slackService;
        

        public SlackAlertService(ResponseBuilder responseBuilder , IMessenger messenger , MessageToNotification messageToNotification , INotificationChannel notificationChannel)
        {
            _messenger = messenger;
            _messageToNotification = messageToNotification;
            _notificationChannel = notificationChannel;
            _slackService = new SlackService(Settings.Instance.SlackBotKey, responseBuilder);
            
        }

        #region Implementation of IHostedService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            
            await _notificationChannel.PostAsync("Morning, Im awake and up and running.");
            await _slackService.Connect();
            MessengerHelper.RegisterAsync<TradeOrderMadeMessage>(_messenger,this, _messageToNotification.OnTradeOrderMade);
        }

      
       

        #endregion
    }
}