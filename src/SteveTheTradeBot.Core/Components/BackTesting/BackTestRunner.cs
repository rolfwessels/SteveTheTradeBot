using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using Serilog;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Tools;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public class BackTestRunner
    {
        private readonly CandleBuilder _candleBuilder;

        public BackTestRunner()
        {
            _candleBuilder = new CandleBuilder();
        }

        public async Task<BackTestResult> Run(IAsyncEnumerable<HistoricalTrade> trades, RSiBot.IBot bot,
            CancellationToken cancellationToken)
        {
            var botData = new BotData {BackTestResult = new BackTestResult {StartingAmount = 1000}};
            
            _candleBuilder.OnMinute = x =>
            {
                botData.ByMinute.Push(x);
                bot.DataReceived(botData);
            };
            await foreach (var trade in trades.WithCancellation(cancellationToken))
                _candleBuilder.Feed(trade);

            return botData.BackTestResult;
        }
    }

    public class BotData
    {
        public BotData()
        {
            ByMinute = new Recent<CandleBuilder.Candle>(1000);
        }

        public Recent<CandleBuilder.Candle> ByMinute { get; }
        public BackTestResult BackTestResult { get; set; }
    }

    public class BackTestResult
    {
        private decimal _startingAmount;
        public List<Trade> Trades { get; } = new List<Trade>();
        public string CurrencyPair { get; set; }
        public int TradesActive => Trades.Count(x => x.IsActive);
        public int TradesMade => Trades.Count(x => !x.IsActive);
        public int TradesSuccesses => Trades.Where(x => !x.IsActive).Count(x => x.Profit > 0);
        public decimal TradesSuccessesPercent => (TradesMade ==0 ?0: Math.Round((decimal) TradesSuccesses / TradesMade * 100m, 2));
        public double AvgDuration => TradesActive >0?0: Trades.Where(x => !x.IsActive).Average(x => (x.EndDate - x.StartDate).Hours);
        public int DatePoints { get; set; }
        public int TotalTransactionCost { get; set; }

        public decimal StartingAmount
        {
            get => _startingAmount;
            set => _startingAmount = Balance = value;
        }

        public decimal Balance { get; set; }

        public Trade AddTrade(in DateTime date, in decimal price, decimal quantity)
        {
            var addTrade = new Trade(date, price, quantity);
            Trades.Add(addTrade);
            return addTrade;
        }

        #region Nested type: Trade

        public class Trade
        {
            public Trade(DateTime startDate, decimal buyPrice, decimal quantity)
            {
                StartDate = startDate;
                BuyPrice = buyPrice;
                Quantity = quantity;
                IsActive = true;
            }

            public decimal Value { get; set; }
            public DateTime StartDate { get; }
            public decimal BuyPrice { get; }
            public decimal Quantity { get; }
            public bool IsActive { get; private set; }
            public decimal SellPrice { get; private set; }
            public DateTime EndDate { get; private set; }
            public decimal Profit { get; set; }


            public Trade Close(in DateTime endDate, in decimal sellPrice)
            {
                EndDate = endDate;
                Value = Math.Round(Quantity * sellPrice, 2);
                SellPrice = sellPrice;
                Profit = (sellPrice - BuyPrice) / BuyPrice * 100;
                IsActive = false;
                return this;
            }
        }

        #endregion
    }


    public class RSiBot : RSiBot.IBot
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int _lookBack;
        private BackTestResult.Trade _activeTrade;
        private readonly int _buySignal;
        private double _initialStopRisk;
        private readonly int _lookBackRequired;
        private readonly int _sellSignal;
        private double _trailingStopRisk;

        public RSiBot(int lookBack = 14)
        {
            _lookBack = lookBack;
            _initialStopRisk = 0.98;
            _trailingStopRisk = 0.90;
            _lookBackRequired = 100 + _lookBack;
            _sellSignal = 80;
            _buySignal = 20;
        }


        public void DataReceived(BotData trade)
        {
            if (trade.ByMinute.Count < _lookBackRequired) return;

            //https://daveskender.github.io/Stock.Indicators/indicators/Rsi/#content
            var rsiResults = RsiResults(trade);
            var currentTrade = trade.ByMinute.Last();
            if (_activeTrade == null)
            {
                if (rsiResults < _buySignal)
                {
                    _log.Information(
                        $"{currentTrade.Date} Send signal to buy at {currentTrade.Close} Rsi:{rsiResults}");

                    _activeTrade = trade.BackTestResult.AddTrade(currentTrade.Date, currentTrade.Close,
                        trade.BackTestResult.Balance / currentTrade.Close);
                }
            }
            else
            {
                if (rsiResults > _sellSignal)
                {
                    _log.Information(
                        $"{currentTrade.Date} Send signal to sell at {currentTrade.Close} Rsi:{rsiResults}");

                    var close = _activeTrade.Close(currentTrade.Date, currentTrade.Close);
                    trade.BackTestResult.Balance = close.Value;
                    _activeTrade = null;
                }
            }
        }

        public static decimal CalculateRelativeStrengthIndex(int numberOfDays, List<CandleBuilder.Candle> historicalDataList)
        {
            
            if (historicalDataList.Count >= numberOfDays)
            {
                decimal totalGain = 0;
                decimal totalLoss = 0;
                var daysUp = 0;
                var daysDown = 0;
                for (int i = 0; i < numberOfDays; i++)
                {
                    decimal changeValue;
                    if (i == 0)
                        changeValue = 0;
                    else
                        changeValue = historicalDataList[i - 1].Close - historicalDataList[i].Close;

                    if (changeValue == 0)
                        continue; //skip
                    if (changeValue > 0)
                    {
                        totalGain = totalGain + historicalDataList[i - 1].Close;
                        daysUp = daysUp + 1;
                    }
                    else
                    {
                        totalLoss = totalLoss + Math.Abs(historicalDataList[i - 1].Close);
                        daysDown = daysDown + 1;
                    }
                }

                decimal relativeStrength;
 
                if (daysDown != 0 && daysUp != 0) //To avoid divide by zero error
                    relativeStrength = (totalGain / daysUp) / (totalLoss / daysDown);
                else if (daysDown != 0) //To avoid divide by zero error
                    relativeStrength = (totalLoss / daysDown);
                else
                    relativeStrength = (totalGain / daysUp);
                var relativeStrengthIndex = 100 - (100 / (1 + relativeStrength));
                return relativeStrengthIndex;
            }

            return 50;
        }

        #region Private Methods

        private decimal RsiResults1(BotData trade)
        {
            var rsiResults = trade.ByMinute.TakeLast(_lookBackRequired).GetRsi(_lookBack).Last();
            return rsiResults.Rsi ?? 50m;
        }

        private decimal RsiResults(BotData trade)
        {
            return CalculateRelativeStrengthIndex(_lookBack,trade.ByMinute);
        }

        #endregion

        #region Nested type: IBot

        public interface IBot
        {
            void DataReceived(BotData trade);
        }

        #endregion
    }
}