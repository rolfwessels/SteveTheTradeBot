using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Serilog;
using Slack.Webhooks;
using SteveTheTradeBot.Core;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Framework.Slack;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Api
{
    public class SlackAlertService : BackgroundService
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IMessenger _messenger;
        private readonly SlackService _slackService;
        private readonly SlackClient _slackClient;

        public SlackAlertService(ResponseBuilder responseBuilder , IMessenger messenger)
        {
            _messenger = messenger;
            _slackService = new SlackService(Settings.Instance.SlackBotKey, responseBuilder);
            _slackClient = new SlackClient(Settings.Instance.SlackWebhookUrl);
        }

    
        #region Implementation of IHostedService

        public override async Task ExecuteAsync(CancellationToken token)
        {
            await _slackClient.PostAsync(Post("Morning, Im awake and up and running."));
            await _slackService.Connect();
            _messenger.Register<TradeOrder>(this,OnTradeOrder);
        }

        private void OnTradeOrder(TradeOrder tradeOrder)
        {
            tradeOrder.Dump("tradeOrder");
            var slackMessage = Post($"I just {tradeOrder.OrderSide} {tradeOrder.OutQuantity} {tradeOrder.OutCurrency}!");
            _slackClient.Post(slackMessage);
        }

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

        #endregion
    }
}