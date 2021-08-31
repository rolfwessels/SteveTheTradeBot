using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBot.ML.Model;

namespace SteveTheTradeBot.Core.Components.Strategies
{
    public class RSiPlusDecisionTreeStrategy : BaseStrategy
    {
        public const string Desc = nameof(RSiPlusDecisionTreeStrategy);

        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int _buySignal;
        private readonly int _quotesToCheckRsi;
        private readonly ConsumeModel _consumeModel;
        private readonly ICloseSignal _closeSignal;


        public RSiPlusDecisionTreeStrategy() : this(new RaiseStopLossCloseSignalDynamic(0.04m))
        {
        }

        public RSiPlusDecisionTreeStrategy(ICloseSignal closeSignal)
        {
            _closeSignal = closeSignal;
            _buySignal = 30;
            _quotesToCheckRsi = 10;
            _consumeModel = new ConsumeModel(Settings.Instance.DecisionTreeStrategyData);
        }

        public override async Task DataReceived(StrategyContext data)
        {
            var currentTrade = data.Quotes.Last();
            var activeTrade = data.ActiveTrade();
            
            var hasRecentlyHitOverSold = Signals.Rsi.HasBuySignal(data.Quotes.TakeLast(_quotesToCheckRsi), _buySignal);
            var predictedGrowth = _consumeModel.Predict(currentTrade.ToModelInput()).Score;
            if (activeTrade == null)
            {

                if (hasRecentlyHitOverSold && predictedGrowth > 1)
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to buy at {currentTrade.Close} Rsi:{hasRecentlyHitOverSold}");
                    var strategyTrade = await Buy(data, data.StrategyInstance.QuoteAmount);
                    var stopLoss = await _closeSignal.Initialize(data, currentTrade.Close,this);
                    data.StrategyInstance.Status =
                        $"Bought! [{strategyTrade.BuyPrice} and set stop loss at {stopLoss}]";
                }
                else
                {
                    data.StrategyInstance.Status =
                        $"Waiting to buy ![wait for min rsi {hasRecentlyHitOverSold} <= {_buySignal} in last {_quotesToCheckRsi}] [isPositiveTrend {predictedGrowth} > 1]";
                }
            }
            else
            {
                await _closeSignal.DetectClose(data, currentTrade, activeTrade,this);
            }
        }

        private bool IsPositiveTrend(IEnumerable<TradeQuote> values)
        {
            decimal lastValue = -1;
            foreach (var value in values)
            {
                if (lastValue != -1 && value.Close <= lastValue)
                {
                    return false;
                }

                lastValue = value.Close;
                
            }

            return true;
        }



        public override string Name => Desc;

       
    }
}