using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.BackTesting
{
    public class BackTestRunner
    {
        public BackTestRunner()
        {
        }

        public async Task<BackTestResult> Run(IAsyncEnumerable<HistoricalTrade> trades, IBot bot, CancellationToken cancellationToken)
        {
            await foreach (var trade in trades.WithCancellation(cancellationToken)
                .ConfigureAwait(false))
                bot.DataReceived(trade);
            return new BackTestResult();
        }
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

        public RSiBot(int lookBack = 14)
        {
            _lookBack = lookBack;
            _initialStopRisk = 0.98;
            _trailingStopRisk = 0.90;
        }

        #region Implementation of IBot

        public Resolution GetRequiredResolution()
        {
            return Resolution.Real;
        }

        public void DataReceived(HistoricalTrade trade)
        {
            
        }

        #endregion
    }

    public interface IBot
    {
        Resolution GetRequiredResolution();
        void DataReceived(HistoricalTrade trade);
    }

    public enum Resolution
    {
        Real,
        Minute,
        Hourly,
        Daily

    }
}
