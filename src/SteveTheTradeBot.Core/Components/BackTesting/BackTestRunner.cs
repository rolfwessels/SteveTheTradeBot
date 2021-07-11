using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
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
            var botData = new BotData();
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

           
            return new BackTestResult();
        }
    }

    public class BotData 
    {
        public BotData()
        {
            ByMinute = new Recent<CandleBuilder.Candle>(1000);
        }

        public Recent<CandleBuilder.Candle> ByMinute { get; }
    }

    public class BackTestResult
    {
        public String CurrencyPair { get; set; }
        public int TradesActive { get; set; }
        public int Trades { get; set; }
        public int TradesSuccesses { get; set; }
        public int DatePoints { get; set; }
        public int TotalTransactionCost { get; set; }
        public int StartingAmount { get; set; }

    }

    public class RSiBot : IBot
    {
        private readonly int _lookBack;
        private double _initialStopRisk;
        private double _trailingStopRisk;
        private int _lookBackRequired;

        public RSiBot(int lookBack = 14)
        {
            _lookBack = lookBack;
            _initialStopRisk = 0.98;
            _trailingStopRisk = 0.90;
            _lookBackRequired = 100 + _lookBack;
        }

        #region Implementation of IBot

       
        public void DataReceived(BotData trade)
        {
            
            if (trade.ByMinute.Count < _lookBackRequired)
            {
                return;
            }

            //https://daveskender.github.io/Stock.Indicators/indicators/Rsi/#content
            var rsiResults = trade.ByMinute.TakeLast(_lookBackRequired).GetRsi(_lookBack).TakeLast(1);
            rsiResults.Dump("results");

        }

        #endregion
    }

    public interface IBot
    {
        void DataReceived(BotData trade);
    }

    
}
