using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Broker;

namespace SteveTheTradeBot.Core.Components.Bots
{
    public class RSiBot : BaseBot
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int _lookBack;
        private readonly int _buySignal;
        private readonly decimal _initialStopRisk;
        private readonly int _lookBackRequired;
        private readonly int _sellSignal;
        private decimal? _setStopLoss;

        public RSiBot(IBrokerApi api, int lookBack = 14) : base(api)
        {
            _lookBack = lookBack;
            _initialStopRisk = 0.95m;
            _lookBackRequired = _lookBack+100;
            _sellSignal = 80;
            _buySignal = 20;
        }
        
        public override async Task DataReceived(BackTestRunner.BotData data)
        {
            if (data.ByMinute.Count < _lookBackRequired) return ;

            //https://daveskender.github.io/Stock.Indicators/indicators/Rsi/#content
            var rsiResults = RsiResults(data);
            var ema = EmaResults(data);
            var currentTrade = data.ByMinute.Last();
            await data.PlotRunData(currentTrade.Date, "rsi", rsiResults);
            await data.PlotRunData(currentTrade.Date, "ema", ema);

            if (ActiveTrade(data) == null)
            {
                if (rsiResults < _buySignal )
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to buy at {currentTrade.Close} Rsi:{rsiResults}");
                    await Buy(data, data.BackTestResult.ClosingBalance);
                    _setStopLoss = currentTrade.Close * _initialStopRisk;
                }
            }
            else
            {
                if (rsiResults > _sellSignal || currentTrade.Close <= _setStopLoss)
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to sell at {currentTrade.Close} Rsi:{rsiResults}");

                    await Sell(data, ActiveTrade(data));
                    _setStopLoss = null;
                    
                }
            }
        }

        private static Trade ActiveTrade(BackTestRunner.BotData trade)
        {
            return trade.BackTestResult.Trades.FirstOrDefault(x=>x.IsActive);
        }


        public override string Name => "SimpleRsi";


        #region Private Methods

        private decimal RsiResults(BackTestRunner.BotData trade)
        {
            var rsiResults = trade.ByMinute.TakeLast(_lookBackRequired).GetRsi(_lookBack).Last();
            return rsiResults.Rsi ?? 50m;
        }


        private decimal EmaResults(BackTestRunner.BotData trade)
        {
            try
            {
                var rsiResults = trade.ByMinute.TakeLast(400).GetEma(200).Last();
                return rsiResults.Ema ?? 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion

      
    }
}