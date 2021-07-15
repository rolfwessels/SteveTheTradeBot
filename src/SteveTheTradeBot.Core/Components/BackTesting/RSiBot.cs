using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using Skender.Stock.Indicators;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public class RSiBot : RSiBot.IBot
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int _lookBack;
        private BackTestResult.Trade _activeTrade;
        private readonly int _buySignal;
        private readonly decimal _initialStopRisk;
        private readonly int _lookBackRequired;
        private readonly int _sellSignal;
        private decimal? _setStopLoss;

        public RSiBot(int lookBack = 14)
        {
            _lookBack = lookBack;
            _initialStopRisk = 0.95m;
            _lookBackRequired = _lookBack+100;
            _sellSignal = 80;
            _buySignal = 20;
        }


        public async Task DataReceived(BackTestRunner.BotData trade)
        {
            if (trade.ByMinute.Count < _lookBackRequired) return ;

            //https://daveskender.github.io/Stock.Indicators/indicators/Rsi/#content
            var rsiResults = RsiResults(trade);
            var ema = EmaResults(trade);
            var currentTrade = trade.ByMinute.Last();
            await trade.PlotRunData(currentTrade.Date, "rsi", rsiResults);
            await trade.PlotRunData(currentTrade.Date, "ema", ema);

            if (_activeTrade == null)
            {
                if (rsiResults < _buySignal && (currentTrade.Close * 2m) > ema)
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to buy at {currentTrade.Close} Rsi:{rsiResults}");

                    _activeTrade = trade.BackTestResult.AddTrade(currentTrade.Date, currentTrade.Close,
                        trade.BackTestResult.ClosingBalance / currentTrade.Close);
                    _setStopLoss = currentTrade.Close * _initialStopRisk;
                    await trade.PlotRunData(currentTrade.Date, "activeTrades", trade.BackTestResult.TradesActive);
                    
                }
            }
            else
            {
                if (rsiResults > _sellSignal || currentTrade.Close <= _setStopLoss)
                {
                    _log.Information(
                        $"{currentTrade.Date.ToLocalTime()} Send signal to sell at {currentTrade.Close} Rsi:{rsiResults}");

                    var close = _activeTrade.Close(currentTrade.Date, currentTrade.Close);
                    trade.BackTestResult.ClosingBalance = close.Value;
                    _setStopLoss = null;
                    _activeTrade = null;
                    await trade.PlotRunData(currentTrade.Date, "activeTrades", trade.BackTestResult.TradesActive);
                    await trade.PlotRunData(currentTrade.Date, "sellPrice", close.Value);
                }
            }
        }

        public string Name => "SimpleRsi";


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

        #region Nested type: IBot

        public interface IBot
        {
            Task DataReceived(BackTestRunner.BotData trade);
            string Name { get;  }
        }

        #endregion
    }
}