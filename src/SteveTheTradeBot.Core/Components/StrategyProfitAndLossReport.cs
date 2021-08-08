using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core.Components
{
    public class StrategyProfitAndLossReport
    {
        private readonly ITradePersistenceFactory _factory;

        public StrategyProfitAndLossReport(ITradePersistenceFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<Record>> Run()
        {
            var context = await _factory.GetTradePersistence();
            return context.Strategies
                .Where(x => x.IsActive && !x.IsBackTest)
                .ToList()
                .OrderBy(x=>x.CurrentQuoteAmount)
                .Select(x => new Record
                {
                    Name = x.Name,
                    CurrentQuoteAmount = x.CurrentQuoteAmount,
                    TotalProfit = TradeUtils.MovementPercent(x.CurrentQuoteAmount,x.InvestmentAmount,1)+"%",
                    MarketProfit = x.PercentMarketProfit,
                    TotalActiveTrades = x.TotalActiveTrades,
                })
                .ToList();
        }

        public class Record
        {
            public string Name { get; set; }
            public decimal CurrentQuoteAmount { get; set; }
            public string TotalProfit { get; set; }
            public decimal MarketProfit { get; set; }
            public decimal TotalActiveTrades { get; set; }
        }
    }
}