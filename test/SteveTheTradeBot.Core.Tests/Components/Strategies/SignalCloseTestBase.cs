using System;
using System.Collections.Generic;
using System.Linq;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Tests.Components.BackTesting;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Components.Strategies
{
    public class SignalCloseTestBase
    {
        protected StrategyContext _strategyContext;
        protected FakeBroker _fakeBroker;

        protected IEnumerable<TradeQuote> BuildOrders(params decimal[] values)
        {
            return values.Select((value, index) => new TradeQuote
            {
                Feed = "valr",
                PeriodSize = PeriodSize.FiveMinutes,
                Date = DateTime.Now.AddDays(-values.Length + index),
                Open = value,
                High = value,
                Low = value,
                Close = value,
                Volume = index,
                CurrencyPair = CurrencyPair.ETHZAR
            });
        }

        protected virtual void Setup()
        {
            var forBackTest = StrategyInstance.ForBackTest("abc",CurrencyPair.XRPZAR,100);
            var dynamicGraphs = new DynamicGraphsTests.FakeGraph();
            _fakeBroker = new FakeBroker(new Messenger());
            _strategyContext = new StrategyContext(dynamicGraphs, forBackTest,_fakeBroker, new Messenger(),new ParameterStore(TestTradePersistenceFactory.InMemoryDb));
        }
    }
}