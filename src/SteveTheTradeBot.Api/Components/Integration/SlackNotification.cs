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
        public SlackNotification()
        {
            _slackClient = new SlackClient(Settings.Instance.SlackWebhookUrl);
        }

        #region Implementation of INotificationChannel

        public async Task PostAsync(string message)
        {
            var slackMessage = new SlackMessage
            {
                Text = message
            };
            await _slackClient.PostAsync(slackMessage);
        }

        #endregion

        private static SlackMessage Post(string message)
        {
            _log.Debug($"SlackAlertService:Post SlackMessage {message}");
            var slackMessage = new SlackMessage
            {
                Text = message
            };
            // var slackAttachment = new SlackAttachment
            // {
            //     Fallback = "New open task [Urgent]: <http://url_to_task|Test out Slack message attachments>",
            //     Text = "New open task *[Urgent]*: <http://url_to_task|Test out Slack message attachments>",
            //     Color = "#D00000",
            //     Fields =
            //         new List<SlackField>
            //         {
            //             new SlackField
            //             {
            //                 Title = "Notes",
            //                 Value = "This is much *easier* than I thought it would be."
            //             }
            //         }
            // };
            // slackMessage.Attachments = new List<SlackAttachment> {slackAttachment};
            return slackMessage;
        }
    }
}