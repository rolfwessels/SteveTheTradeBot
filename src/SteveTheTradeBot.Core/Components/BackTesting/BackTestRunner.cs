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

        public async Task<BackTestResult> Run(IEnumerable<HistoricalTrade> trades, RSiBot.IBot bot,
            CancellationToken cancellationToken)
        {
            var botData = new BotData {BackTestResult = new BackTestResult {StartingAmount = 1000}};
           
            foreach (var trade in trades.ToCandleOneMinute().Aggregate(PeriodSize.FiveMinutes))
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (botData.BackTestResult.MarketOpenAt == 0) botData.BackTestResult.MarketOpenAt = trade.Close;
                botData.ByMinute.Push(trade);
                bot.DataReceived(botData);
                botData.BackTestResult.MarketClosedAt = trade.Close;
            }

            return botData.BackTestResult;
        }
    }

    public class BotData
    {
        public BotData()
        {
            ByMinute = new Recent<IQuote>(1000);
        }

        public Recent<IQuote> ByMinute { get; }
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
        public decimal MarketOpenAt { get; set; }
        public decimal MarketClosedAt { get; set; }

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
                    _setStopLoss = currentTrade.Close * _initialStopRisk;
                }
            }
            else
            {
                if (rsiResults > _sellSignal || currentTrade.Close <= _setStopLoss)
                {
                    _log.Information(
                        $"{currentTrade.Date} Send signal to sell at {currentTrade.Close} Rsi:{rsiResults}");

                    var close = _activeTrade.Close(currentTrade.Date, currentTrade.Close);
                    trade.BackTestResult.Balance = close.Value;
                    _setStopLoss = null;
                    _activeTrade = null;
                }
            }
        }


        #region Private Methods

        private decimal RsiResults(BotData trade)
        {
            var rsiResults = trade.ByMinute.TakeLast(_lookBackRequired).GetRsi(_lookBack).Last();
            return rsiResults.Rsi ?? 50m;
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
/*
 * // Output the results
 http://www.modulusfe.com/products/trading-system-developer-components/back-test-lib/

Console .WriteLine( "Total number of trades: {0:G}" , results.TotalNumberOfTrades);

Console .WriteLine( "Average number of trades per month: {0:G}" , results.AverageTradesPerMonth);

Console .WriteLine( "Total number of profitable trades: {0:G}" , results.NumberOfProfitableTrades);

Console .WriteLine( "Total number of losing trades: {0:G}" , results.NumberOfLosingTrades);

Console .WriteLine( "Total profit: {0:C}" , results.TotalProfit);

Console .WriteLine( "Total loss: {0:C}" , results.TotalLoss);

Console .WriteLine( "Percent profitable trades: {0:P}" , results.PercentProfit);

Console .WriteLine( "Percent profitable trades: {0:P}" , results.PercentProfit);

Console .WriteLine( "Largest profit: {0:C}" , results.LargestProfit);

Console .WriteLine( "Largest loss: {0:C}" , results.LargestLoss);

Console .WriteLine( "Maximum drawdown: {0:C}" , results.MaximumDrawDown);

Console .WriteLine( "Maximum drawdown Monte Carlo: {0:C}" , results.MaximumDrawDownMonteCarlo);

Console .WriteLine( "Standard deviation: {0:G}" , results.StandardDeviation);

Console .WriteLine( "Standard deviation annualized: {0:G}" , results.StandardDeviationAnnualized);

Console .WriteLine( "Downside deviation (MAR = 10%): {0:G}" , results.DownsideDeviationMar10);

Console .WriteLine( "Value Added Monthly Index (VAMI): {0:G}" , results.ValueAddedMonthlyIndex);

Console .WriteLine( "Sharpe ratio: {0:G}" , results.SharpeRatio);

Console .WriteLine( "Sortino ratio: {0:G}" , results.SortinoRatioMAR5);

Console .WriteLine( "Annualized Sortino ratio: {0:G}" , results.AnnualizedSortinoRatioMAR5);

Console .WriteLine( "Sterling ratio: {0:G}" , results.SterlingRatioMAR5);

Console .WriteLine( "Calmar ratio: {0:G}" , results.CalmarRatio);

Console .WriteLine( "Risk to reward ratio: {0:P}" , results.RiskRewardRatio);
 */
