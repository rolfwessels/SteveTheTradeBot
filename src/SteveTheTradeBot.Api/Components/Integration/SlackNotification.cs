using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using Slack.Webhooks;
using SteveTheTradeBot.Core;
using SteveTheTradeBot.Core.Components.Notifications;

namespace SteveTheTradeBot.Api.Components.Integration
{
    public class SlackNotification : INotificationChannel
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly SlackClient _slackClient;
        private static string _channel;

        public SlackNotification()
        {
            _slackClient = new SlackClient(Settings.Instance.SlackWebhookUrl);
            _channel = Settings.Instance.SlackChannel;
        }

        #region Implementation of INotificationChannel

        public async Task PostAsync(string message)
        {
            var slackMessage = DefaultSlackMessage(message);
            _log.Debug($"SlackNotification:PostAsync {slackMessage.Text}");

            await _slackClient.PostAsync(slackMessage);
        }

        public Task PostSuccessAsync(string message)
        {
            var slackMessage = PostTextAttachment(message, "#49C39e");
            return _slackClient.PostAsync(slackMessage);
        }


        public Task PostFailedAsync(string message)
        {
            var slackMessage = PostTextAttachment(message, "#D00000");
            return _slackClient.PostAsync(slackMessage);
        }


        private static SlackMessage PostTextAttachment(string message, string d00000)
        {
            _log.Debug($"SlackNotification:PostTextAttachment {message}");
            var slackMessage = DefaultSlackMessage();
            var slackAttachment = new SlackAttachment
            {
                Fallback = message,
                Text = message,
                Color = d00000,
            };
            slackMessage.Attachments = new List<SlackAttachment> {slackAttachment};
            return slackMessage;
        }

        private static SlackMessage DefaultSlackMessage(string message = null)
        {
            var slackMessage = new SlackMessage()
            {
                Channel = _channel,
                Text = message
            };
            return slackMessage;
        }

        #endregion

    }
}