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
        private CandleBuilder _candleBuilder;

        public BackTestRunner()
        {
            _candleBuilder = new CandleBuilder();
        }

        public async Task<BackTestResult> Run(IAsyncEnumerable<HistoricalTrade> trades, IBot bot, CancellationToken cancellationToken)
        {

            var botData = new BotData {BackTestResult = new BackTestResult() { StartingAmount = 1000 } };
            _candleBuilder.OnMinute = x =>
            {
                botData.ByMinute.Push(x);
                bot.DataReceived(botData);
            };
            await foreach (var trade in trades.WithCancellation(cancellationToken)
                .ConfigureAwait(false))
            {
                _candleBuilder.Feed(trade);
            }

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
        public String CurrencyPair { get; set; }
        public int TradesActive => Trades.Count(x => x.IsActive);
        public int TradesMade => Trades.Count(x => !x.IsActive);
        public int TradesSuccesses => Trades.Where(x => !x.IsActive).Count(x => x.Profit > 0);
        public decimal TradesSuccessesPercent => Math.Round((decimal)TradesSuccesses / TradesMade * 100m, 2);
        public double AvgDuration => Trades.Where(x=>!x.IsActive).Average(x => (x.EndDate - x.StartDate).Hours );
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

        public class Trade
        {
            public decimal Value { get; set; }
            public DateTime StartDate { get; }
            public decimal BuyPrice { get; }
            public decimal Quantity { get; }
            public bool IsActive { get; private set; }
            public decimal SellPrice { get; private set; }
            public DateTime EndDate { get; private set; }
            public decimal Profit { get; set; }
            public Trade(DateTime startDate,  decimal buyPrice, decimal quantity)
            {
                StartDate = startDate;
                BuyPrice = buyPrice;
                Quantity = quantity;
                IsActive = true;
            }


            public Trade Close(in DateTime endDate, in decimal sellPrice)
            {
                EndDate = endDate;
                Value = Math.Round(Quantity * sellPrice,2);
                SellPrice = sellPrice;
                Profit = ((sellPrice - BuyPrice) / BuyPrice) * 100;
                IsActive = false;
                return this;
            }

            
        }
    }

    

    public class RSiBot : IBot
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int _lookBack;
        private double _initialStopRisk;
        private double _trailingStopRisk;
        private int _lookBackRequired;
        private int _sellSignal;
        private int _buySignal;
        private BackTestResult.Trade _activeTrade;

        public RSiBot(int lookBack = 14)
        {
            _lookBack = lookBack;
            _initialStopRisk = 0.98;
            _trailingStopRisk = 0.90;
            _lookBackRequired = 100 + _lookBack;
            _sellSignal = 80;
            _buySignal = 20;
        }

        #region Implementation of IBot

       
        public void DataReceived(BotData trade)
        {
            
            if (trade.ByMinute.Count < _lookBackRequired)
            {
                return;
            }

            //https://daveskender.github.io/Stock.Indicators/indicators/Rsi/#content
            var rsiResults = trade.ByMinute.TakeLast(_lookBackRequired).GetRsi(_lookBack).Last();
            var currentTrade = trade.ByMinute.Last();
            if (_activeTrade == null)
            {
                if (rsiResults.Rsi < _buySignal)
                {
                    
                    _log.Information(
                        $"{currentTrade.Date} Send signal to buy at {currentTrade.Close} Rsi:{rsiResults.Rsi}");

                    _activeTrade = trade.BackTestResult.AddTrade(currentTrade.Date, currentTrade.Close, trade.BackTestResult.Balance / currentTrade.Close);
                }
            }
            else
            {
                if (rsiResults.Rsi > _sellSignal)
                {

                    _log.Information(
                        $"{currentTrade.Date} Send signal to sell at {currentTrade.Close} Rsi:{rsiResults.Rsi}");

                    var close = _activeTrade.Close(currentTrade.Date, currentTrade.Close);
                    trade.BackTestResult.Balance = close.Value;
                    _activeTrade = null;
                }
            }



        }

        #endregion
    }

    public interface IBot
    {
        void DataReceived(BotData trade);
    }

    
}
