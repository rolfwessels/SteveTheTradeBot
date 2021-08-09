using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SlackConnector.Models;
using SteveTheTradeBot.Core.Components.SlackResponders;
using SteveTheTradeBot.Core.Framework.Settings;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public class HelpResponder : ResponderBase
    {
        private readonly IEnumerable<IResponder> _responderDescriptions;

        public HelpResponder(IEnumerable<IResponder> responderDescriptions)
        {
            _responderDescriptions = responderDescriptions;
        }

        #region Overrides of ResponderBase

        public override bool CanRespond(MessageContext context)
        {
            return context.IsForBot() && ( context.HasMessage("help") || context.HasMessage("hi") );
        }

        #endregion

        #region Implementation of IResponder

        public override async Task GetResponse(MessageContext context)
        {
            var botMessage = new BotMessage() { Text =
                $"{SlackHelper.GetGreeting()}, You are currently connected to v{ConfigurationBuilderHelper.InformationalVersion()}-{ConfigurationBuilderHelper.GetEnvironment().ToLower()}\n\n{GetCommands()}"
            };
            await context.Say(botMessage);
        }

        private string GetCommands()
        {
            var stringBuilder = new StringBuilder();
            foreach (var responderDescription in _responderDescriptions)
            {
                if (responderDescription is IResponderDescription description)
                    AppendDescription(stringBuilder, description);
                var descriptions = responderDescription as IResponderDescriptions;
                if (descriptions == null) continue;
                foreach (var desc in descriptions.Descriptions)
                {
                    AppendDescription(stringBuilder, desc);
                }
            }
            return stringBuilder.ToString();
        }

        private static void AppendDescription(StringBuilder stringBuilder, IResponderDescription description)
        {
            stringBuilder.AppendLine($"*{description.Command}* {description.Description}");
        }

     

        #endregion
    }

    
}