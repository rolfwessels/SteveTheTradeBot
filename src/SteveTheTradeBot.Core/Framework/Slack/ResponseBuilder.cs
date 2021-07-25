using System.Collections.Generic;
using SlackConnector.Models;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public class ResponseBuilder
    {
        private IMessenger messenger;

        public ResponseBuilder(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public  List<IResponder> GetResponders()
        {
            var responders = new List<IResponder> {
                new Register(messenger),
                new HelpResponder(new IResponder[0])
            };
            
            
            return responders;
        }
    }

    public class Register : ResponderBase , IResponderDescription
    {
        private readonly IMessenger _messenger;

        public Register(IMessenger messenger)
        {
            _messenger = messenger;
        }

        #region Overrides of ResponderBase

        public override bool CanRespond(MessageContext context)
        {
            return base.CanRespond(context) && context.MessageContains("register");
        }

        public override BotMessage GetResponse(MessageContext context)
        {
            context.Say("Hi!");
            _messenger.Register<TradeOrder>(this,x=> context.Say($"Order placed {x.OrderSide} for {x.OutQuantity} {x.OutCurrency} with value of {x.OriginalQuantity} {x.FeeCurrency}."));
            return null;
        }

        #endregion

        #region Implementation of IResponderDescription

        public string Command => "register";
        public string Description => "Register where to respond";

        #endregion
    }
}