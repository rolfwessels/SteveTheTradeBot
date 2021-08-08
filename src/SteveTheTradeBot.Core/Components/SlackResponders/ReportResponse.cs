using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SlackConnector.Models;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.Slack;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.SlackResponders
{
    public class ReportResponse : ResponderBase , IResponderDescription
    {
        private readonly ITradePersistenceFactory _factory;

        public ReportResponse(ITradePersistenceFactory factory)
        {
            _factory = factory;
        }

        #region Overrides of ResponderBase

        public override bool CanRespond(MessageContext context)
        {
            return base.CanRespond(context) && context.MessageContains(Command);
        }

        public override async Task GetResponse(MessageContext context)
        {
            var strategyProfitAndLossReport = new StrategyProfitAndLossReport(_factory);
            var task = await strategyProfitAndLossReport.Run();
            await context.SayCode(task.ToTable());
        }

        #endregion

        #region Implementation of IResponderDescription

        public string Command => "report";
        public string Description => "Prints reports";

        #endregion
    }
}