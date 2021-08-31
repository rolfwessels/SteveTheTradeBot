using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.BackTesting;
using SteveTheTradeBot.Core.Components.Strategies;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Tests.Components.Storage;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Tests.Components.BackTesting
{
    [Category("FullIntegration")]
    public class BackTestRunnerTests
    {

        [Test]
        [Timeout(240000)]
        public void Fast_MacdStrategy_OverPeriods()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);

            var strategyInstances = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.Week).Select(x =>
                BuildBackTestResult(@from, to, new MacdStrategy(), x.Item2.CurrencyPair, x.Item1).Result);

            var table = strategyInstances.Select(x => new
            {
                x.Pair, x.PeriodSize, x.PercentMarketProfit, x.PercentProfit, x.AverageTradesPerMonth,
                x.PercentOfProfitableTrades
            }).ToTable();
            Console.Out.WriteLine(table);

            // ╔════════╦════════════════╦═════════════════════╦═══════════════╦═══════════════════════╦═══════════════════════════╗                    
            // ║ Pair   ║ PeriodSize     ║ PercentMarketProfit ║ PercentProfit ║ AverageTradesPerMonth ║ PercentOfProfitableTrades ║                    
            // ╠════════╬════════════════╬═════════════════════╬═══════════════╬═══════════════════════╬═══════════════════════════╣                    
            // ║ BTCZAR ║ OneMinute      ║ 37.812              ║ 35.196        ║ 6.429                 ║ 16.67                     ║                    
            // ║ BTCZAR ║ FiveMinutes    ║ 37.258              ║ 39.188        ║ 4.286                 ║ 50.0                      ║                    
            // ║ BTCZAR ║ FifteenMinutes ║ 38.032              ║ 45.576        ║ 3.214                 ║ 66.67                     ║                    
            // ║ BTCZAR ║ ThirtyMinutes  ║ 37.503              ║ 54.988        ║ 2.143                 ║ 100                       ║                    
            // ║ BTCZAR ║ OneHour        ║ 37.019              ║ 53.130        ║ 3.214                 ║ 66.67                     ║                    
            // ║ BTCZAR ║ Day            ║ 33.584              ║ 0             ║ 0                     ║ 0                         ║                    
            // ║ ETHZAR ║ OneMinute      ║ 8.204               ║ 0.020         ║ 17.146                ║ 43.75                     ║                    
            // ║ ETHZAR ║ FiveMinutes    ║ 8.199               ║ 5.594         ║ 8.571                 ║ 37.50                     ║                    
            // ║ ETHZAR ║ FifteenMinutes ║ 7.767               ║ 0.448         ║ 5.357                 ║ 40.0                      ║                    
            // ║ ETHZAR ║ ThirtyMinutes  ║ 7.323               ║ 12.214        ║ 3.214                 ║ 66.67                     ║                    
            // ║ ETHZAR ║ OneHour        ║ 7.305               ║ 39.460        ║ 3.214                 ║ 66.67                     ║                    
            // ║ ETHZAR ║ Day            ║ 3.296               ║ 0             ║ 0                     ║ 0                         ║                    
            // ╚════════╩════════════════╩═════════════════════╩═══════════════╩═══════════════════════╩═══════════════════════════╝ 
        }

        [Test]
        [Timeout(240000)]
        public void Fast_RSiMlStrategy_OverPeriods()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);

            var strategyInstances = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.Week).Select(x =>
                BuildBackTestResult(@from, to, new RSiPlusDecisionTreeStrategy(), x.Item2.CurrencyPair, x.Item1).Result);

            var table = strategyInstances.Select(x => new
            {
                x.Pair, x.PeriodSize, x.PercentMarketProfit, x.PercentProfit, x.AverageTradesPerMonth,
                x.PercentOfProfitableTrades
            }).ToTable();
            Console.Out.WriteLine(table);

        }

        [Test]
        [Timeout(240000)]
        public void Fast_RSiConfirmStrategy_OverPeriods()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);

            var strategyInstances = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.Week).Select(x =>
                BuildBackTestResult(@from, to, new RSiConfirmStrategy(), x.Item2.CurrencyPair, x.Item1).Result);

            var table = strategyInstances.Select(x => new
            {
                x.Pair,
                x.PeriodSize,
                x.PercentMarketProfit,
                x.PercentProfit,
                x.AverageTradesPerMonth,
                x.PercentOfProfitableTrades
            }).ToTable();
            Console.Out.WriteLine(table);
// ╔════════╦════════════════╦═════════════════════╦═══════════════╦═══════════════════════╦═══════════════════════════╗                    
// ║ Pair   ║ PeriodSize     ║ PercentMarketProfit ║ PercentProfit ║ AverageTradesPerMonth ║ PercentOfProfitableTrades ║                    
// ╠════════╬════════════════╬═════════════════════╬═══════════════╬═══════════════════════╬═══════════════════════════╣                    
// ║ BTCZAR ║ OneMinute      ║ 37.812              ║ 38.150        ║ 15.001                ║ 50.0                      ║                    
// ║ BTCZAR ║ FiveMinutes    ║ 37.258              ║ 47.954        ║ 9.643                 ║ 44.44                     ║                    
// ║ BTCZAR ║ FifteenMinutes ║ 38.032              ║ 30.058        ║ 7.5                   ║ 57.14                     ║                    
// ║ BTCZAR ║ ThirtyMinutes  ║ 37.503              ║ 15.285        ║ 4.286                 ║ 50.0                      ║                    
// ║ BTCZAR ║ OneHour        ║ 37.019              ║ -2.688        ║ 4.286                 ║ 50.0                      ║                    
// ║ BTCZAR ║ Day            ║ 33.584              ║ 0             ║ 0                     ║ 0                         ║                    
// ║ ETHZAR ║ OneMinute      ║ 8.204               ║ 29.668        ║ 23.576                ║ 63.64                     ║                    
// ║ ETHZAR ║ FiveMinutes    ║ 8.199               ║ 77.844        ║ 16.071                ║ 66.67                     ║                    
// ║ ETHZAR ║ FifteenMinutes ║ 7.767               ║ -1.735        ║ 3.214                 ║ 33.33                     ║                    
// ║ ETHZAR ║ ThirtyMinutes  ║ 7.323               ║ 8.183         ║ 7.5                   ║ 42.86                     ║                    
// ║ ETHZAR ║ OneHour        ║ 7.305               ║ 25.994        ║ 6.429                 ║ 50.0                      ║                    
// ║ ETHZAR ║ Day            ║ 3.296               ║ 0             ║ 0                     ║ 0                         ║                    
// ╚════════╩════════════════╩═════════════════════╩═══════════════╩═══════════════════════╩═══════════════════════════╝                    

        }

        [Test]
        [Timeout(240000)]
        public void Fast_RSiConfirmTrendStrategy_OverPeriods()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);

            var strategyInstances = ValrFeeds.AllWithPeriods().Where(x => x.Item1 != PeriodSize.Week && x.Item1 != PeriodSize.Day).Select(x =>
                BuildBackTestResult(@from, to, new RSiConfirmTrendStrategy(), x.Item2.CurrencyPair, x.Item1).Result);

            var table = strategyInstances.Select(x => new
            {
                x.Pair,
                x.PeriodSize,
                x.PercentMarketProfit,
                x.PercentProfit,
                x.AverageTradesPerMonth,
                x.PercentOfProfitableTrades
            }).ToTable();
            Console.Out.WriteLine(table);
        // ╔════════╦════════════════╦═════════════════════╦═══════════════╦═══════════════════════╦═══════════════════════════╗                    
        // ║ Pair   ║ PeriodSize     ║ PercentMarketProfit ║ PercentProfit ║ AverageTradesPerMonth ║ PercentOfProfitableTrades ║                    
        // ╠════════╬════════════════╬═════════════════════╬═══════════════╬═══════════════════════╬═══════════════════════════╣                    
        // ║ BTCZAR ║ OneMinute      ║ 37.812              ║ 34.597        ║ 27.859                ║ 34.62                     ║                    
        // ║ BTCZAR ║ FiveMinutes    ║ 37.258              ║ 31.921        ║ 15                    ║ 50.0                      ║                    
        // ║ BTCZAR ║ FifteenMinutes ║ 38.032              ║ 10.783        ║ 10.714                ║ 40.0                      ║                    
        // ║ BTCZAR ║ ThirtyMinutes  ║ 37.503              ║ -0.451        ║ 3.214                 ║ 33.33                     ║                    
        // ║ BTCZAR ║ OneHour        ║ 37.019              ║ 1.345         ║ 0                     ║ 100                       ║                    
        // ║ BTCZAR ║ Day            ║ 33.584              ║ 0             ║ 0                     ║ 0                         ║                    
        // ║ ETHZAR ║ OneMinute      ║ 8.204               ║ -1.428        ║ 47.152                ║ 22.73                     ║                    
        // ║ ETHZAR ║ FiveMinutes    ║ 8.199               ║ 3.562         ║ 20.357                ║ 57.89                     ║                    
        // ║ ETHZAR ║ FifteenMinutes ║ 7.767               ║ -0.899        ║ 7.5                   ║ 14.29                     ║                    
        // ║ ETHZAR ║ ThirtyMinutes  ║ 7.323               ║ -1.332        ║ 0                     ║ 0                         ║                    
        // ║ ETHZAR ║ OneHour        ║ 7.305               ║ -1.487        ║ 4.286                 ║ 25.00                     ║                    
        // ║ ETHZAR ║ Day            ║ 3.296               ║ 0             ║ 0                     ║ 0                         ║                    
        // ╚════════╩════════════════╩═════════════════════╩═══════════════╩═══════════════════════╩═══════════════════════════╝  

        }

        [Test]
        [Timeout(240000)]
        [Ignore("cant get it lower")]
        public async Task Fast_RSiStrategy()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 39;
            await Test(@from, to, expected, new RSiStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }

        [Test]
        [Timeout(240000)]
        public async Task Fast_RSiMslStrategy()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 42;
            await Test(@from, to, expected, new RSiMslStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }

        [Test]
        [Timeout(240000)]
        public async Task Fast_RSiConfirmStrategy()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 43;
            await Test(@from, to, expected, new RSiConfirmStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }


        [Test]
        [Timeout(240000)]
        public async Task Fast_RSiConfirmTrendStrategy()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 12; // failing unless we take the trend part
            await Test(@from, to, expected, new RSiConfirmTrendStrategy(), CurrencyPair.BTCZAR, PeriodSize.FifteenMinutes);
        }


        [Test]
        [Timeout(240000)]
        public async Task Fast_RSiConfirmTrendStrategy_ETHZAR()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 5; // failing unless we take the trend part
            await Test(@from, to, expected, new RSiConfirmTrendStrategy(), CurrencyPair.ETHZAR, PeriodSize.FiveMinutes);
        }

        [Test]
        [Timeout(240000)]
        public async Task Fast_MacdStrategy()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2021-02-01T00:00:00");
            var to = from.AddMonths(1);
            var expected = 47;
            await Test(@from, to, expected, new MacdStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }



        [Test]
        [Timeout(240000)]
        public async Task Run_GivenRSiStrategy_ShouldOver2YearsShouldMake68PlusProfit() 
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 65; // failing
            await Test(@from, to, expected, new RSiConfirmTrendStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }


        [Test]
        [Timeout(240000)]
        [Explicit]
        public async Task Run_GivenRSiMslStrategy_ShouldOver2YearsShouldMake470PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 470; 
            await Test(@from, to, expected, new RSiMslStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }


        [Test]
        [Timeout(240000)]
        [Explicit]
        public async Task Run_GivenRSiConfirmStrategy_ShouldOver2YearsShouldMake1000PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 367; // currently says 1918.354 but I think that is BS
            await Test(@from, to, expected, new RSiConfirmStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }


        [Test]
        [Timeout(240000)]
        [Explicit]
        public async Task Run_GivenMacdStrategy_ShouldOver2YearsShouldMake470PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2019-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 470; // currently says 1953.354 but I think that is BS
            await Test(@from, to, expected,new MacdStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }


        [Test]
        [Timeout(240000)]
        public async Task Run_GivenRSiMslStrategy_ShouldOver1YearsShouldMake200PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2020-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 30; // 
            await Test(@from, to, expected, new RSiMslStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }

        [Test]
        [Timeout(240000)]
        public async Task Run_GivenRSiPlusDecisionTreeStrategy_ShouldOver1YearsShouldMake200PlusProfit()
        {
            // arrange
            Setup();
            var from = DateTime.Parse("2020-11-01T00:00:00");
            var to = DateTime.Parse("2021-07-21T00:00:00");
            var expected = 8; // 
            await Test(@from, to, expected, new RSiPlusDecisionTreeStrategy(), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes);
        }

        [Test]
        [Timeout(240000)]
        public async Task CompareStrategies()
        {
            // arrange
            Setup();
            // The current prod trade
            var from = DateTime.Parse("2021/08/10 10:10:00z");
            var to = DateTime.Now;
            // var from = DateTime.Parse("2019-11-01T00:00:00");
            // var to = DateTime.Parse("2021-07-21T00:00:00");
            var allStrategies = new BaseStrategy[] { new RSiConfirmTrendStrategy(), new RSiPlusDecisionTreeStrategy(), new RSiConfirmStrategy(), new RSiMslStrategy(), new RSiStrategy() , new MacdStrategy()   };
            var enumerable = allStrategies.Select(strategy => BuildBackTestResult(@from, to,strategy, CurrencyPair.BTCZAR, PeriodSize.FiveMinutes, 500));
            var strategyInstances = await Task.WhenAll(enumerable);
            strategyInstances
                .OrderByDescending(x => x.PercentProfit)
                .Select(x=>new { x.StrategyName, ProfitOverMarket = x.PercentProfit- x.PercentMarketProfit , x.PercentProfit , x.TotalNumberOfTrades, x.PercentOfProfitableTrades}).PrintTable();
            strategyInstances.First(x => x.StrategyName == "RSiConfirmStrategy").Print();
        }

        [Test]
        [Timeout(240000)]
        public async Task CompareCloseStrategies()
        {
            // arrange
            Setup();
            // The current prod trade
            var from = DateTime.Parse("2021/08/10 10:10:00z");
            var to = DateTime.Parse("2021/08/25 06:15:00z");
            // var from = DateTime.Parse("2019-11-01T00:00:00");
            // var to = DateTime.Parse("2021-07-21T00:00:00");

// ╔═════════════════════════════════════════════════╦═════════════════════════════╦══════════════════╦═══════════════╦═════════════════════╦═══════════════════════════╗
// ║ Name                                            ║ StrategyName                ║ ProfitOverMarket ║ PercentProfit ║ TotalNumberOfTrades ║ PercentOfProfitableTrades ║
// ╠═════════════════════════════════════════════════╬═════════════════════════════╬══════════════════╬═══════════════╬═════════════════════╬═══════════════════════════╣
// ║ RaiseManualStopLossCloseSignal                  ║ RSiPlusDecisionTreeStrategy ║ 733.773          ║ 939.884       ║ 159                 ║ 46.54                     ║
// ║ RaiseStopLossCloseSignal                        ║ RSiPlusDecisionTreeStrategy ║ -126.751         ║ 79.360        ║ 181                 ║ 43.65                     ║
// ║ RaiseStopLossCloseSignalDynamic[0.04,0.01,0.05] ║ RSiPlusDecisionTreeStrategy ║ -151.847         ║ 54.264        ║ 265                 ║ 35.85                     ║
// ║ RaiseStopLossCloseSignalDynamic[0.03,0.01,0.05] ║ RSiPlusDecisionTreeStrategy ║ -197.567         ║ 8.544         ║ 341                 ║ 37.83                     ║
// ║ RaiseStopLossCloseSignalDynamic[0.02,0.01,0.05] ║ RSiPlusDecisionTreeStrategy ║ -268.603         ║ -62.492       ║ 453                 ║ 36.64                     ║
// ║ RaiseStopLossCloseSignalDynamic[0.02,0.01,0.05] ║ RSiPlusDecisionTreeStrategy ║ -268.603         ║ -62.492       ║ 453                 ║ 36.64                     ║
// ║ MacdCloseSignal                                 ║ RSiPlusDecisionTreeStrategy ║ -290.295         ║ -84.184       ║ 627                 ║ 25.20                     ║
// ║ RaiseStopLossCloseSignalDynamic[0.01,0.01,0.06] ║ RSiPlusDecisionTreeStrategy ║ -298.389         ║ -92.278       ║ 662                 ║ 33.23                     ║
// ║ RaiseStopLossCloseSignalDynamic[0.01,0.01,0.07] ║ RSiPlusDecisionTreeStrategy ║ -298.469         ║ -92.358       ║ 646                 ║ 33.28                     ║
// ║ RaiseStopLossCloseSignalDynamic[0.01,0.01,0.09] ║ RSiPlusDecisionTreeStrategy ║ -298.641         ║ -92.530       ║ 595                 ║ 31.93                     ║
// ║ RaiseStopLossCloseSignalDynamic[0.01,0.01,0.05] ║ RSiPlusDecisionTreeStrategy ║ -298.749         ║ -92.638       ║ 690                 ║ 33.19                     ║
// ║ RaiseStopLossCloseSignalDynamic[0.01,0.01,0.08] ║ RSiPlusDecisionTreeStrategy ║ -299.265         ║ -93.154       ║ 617                 ║ 32.41                     ║
// ║ RaiseStopLossCloseSignalDynamic[0.01,0.01,0.03] ║ RSiPlusDecisionTreeStrategy ║ -299.783         ║ -93.672       ║ 737                 ║ 33.51                     ║
// ║ RaiseStopLossCloseSignalDynamic[0.01,0.01,0.04] ║ RSiPlusDecisionTreeStrategy ║ -299.955         ║ -93.844       ║ 719                 ║ 33.24                     ║
// ║ DynamicStopLossAndProfitCloseSignal             ║ RSiPlusDecisionTreeStrategy ║ -302.813         ║ -96.702       ║ 610                 ║ 27.87                     ║
// ╚═════════════════════════════════════════════════╩═════════════════════════════╩══════════════════╩═══════════════╩═════════════════════╩═══════════════════════════╝

            var allStrategies = new Func<ICloseSignal, BaseStrategy>[] { cs=> new RSiPlusDecisionTreeStrategy(cs) };
            var allCloseSignals = new ICloseSignal[] { new RaiseStopLossCloseSignal(),new RaiseManualStopLossCloseSignal(), new MacdCloseSignal(), new DynamicStopLossAndProfitCloseSignal(), 
                new RaiseStopLossCloseSignalDynamic(), 
                new RaiseStopLossCloseSignalDynamic(0.01m, 0.01m, 0.05m),
                new RaiseStopLossCloseSignalDynamic(0.02m, 0.01m, 0.05m),
                new RaiseStopLossCloseSignalDynamic(0.03m, 0.01m, 0.05m),
                new RaiseStopLossCloseSignalDynamic(0.04m, 0.01m, 0.05m),
                new RaiseStopLossCloseSignalDynamic(0.01m, 0.01m, 0.03m),
                new RaiseStopLossCloseSignalDynamic(0.01m, 0.01m, 0.04m),
                new RaiseStopLossCloseSignalDynamic(0.01m, 0.01m, 0.06m),
                new RaiseStopLossCloseSignalDynamic(0.01m, 0.01m, 0.07m),
                new RaiseStopLossCloseSignalDynamic(0.01m, 0.01m, 0.08m),
                new RaiseStopLossCloseSignalDynamic(0.01m, 0.01m, 0.09m)

            };
            var enumerable = allStrategies
                .SelectMany(strategy => allCloseSignals.Select((cs,i) => new { br = BuildBackTestResult(@from, to, strategy(cs), CurrencyPair.BTCZAR, PeriodSize.FiveMinutes, 500, cs.Name+"i"+i) , cs = cs}) )
                .ToList();
            await Task.WhenAll(enumerable.Select(x=>x.br));
            enumerable
                .OrderByDescending(x => x.br.Result.PercentProfit)
                .Select(x => new { 
                    x.cs.Name,
                    x.br.Result.StrategyName, ProfitOverMarket = x.br.Result.PercentProfit - x.br.Result.PercentMarketProfit, x.br.Result.PercentProfit, x.br.Result.TotalNumberOfTrades, x.br.Result.PercentOfProfitableTrades }).PrintTable();
        }

        private async Task Test(DateTime fromDate, DateTime to, decimal expected,
            IStrategy getStrategy, string currencyPair, PeriodSize size, int amount = 1000)
        {
            var backTestResult = await BuildBackTestResult(fromDate, to, getStrategy, currencyPair, size, amount);
            // assert
            Console.Out.WriteLine($"{fromDate.ToLocalTime()} to {to} {(to - fromDate).ToShort()} ");
            backTestResult.Print();

            backTestResult.PercentProfit.Should().BeApproximately(expected,1);
        }

        private async Task<StrategyInstance> BuildBackTestResult(DateTime fromDate, DateTime to,
            IStrategy strategy, string currencyPair,
            PeriodSize size, int amount = 1000 , string closeSignalName = null)
        {
            var factory = TestTradePersistenceFactory.RealDb();
            var tradeHistoryStore = new TradeHistoryStore(factory);
            var tradeFeedCandleStore = new TradeQuoteStore(factory);
            var strategyInstanceStore = new StrategyInstanceStore(factory);
            var parameterStore = new ParameterStore(factory);
            var player = new HistoricalDataPlayer(tradeHistoryStore, tradeFeedCandleStore);

            var fakeBroker = new FakeBroker(Messenger.Default, tradeHistoryStore);
            var picker = new StrategyPicker().Add(strategy.Name, () => strategy);


            var strategyInstance = StrategyInstance.ForBackTest(strategy.Name, currencyPair, amount, size);
            strategyInstance.Reference += closeSignalName+$"{fromDate:yyMM}-{to:yyMM}";
            await strategyInstanceStore.RemoveByReference(strategyInstance.Reference);
            await strategyInstanceStore.Add(strategyInstance);

            var dynamicGraphs = new DynamicGraphs(factory);

            var strategyRunner = new StrategyRunner(picker, dynamicGraphs, strategyInstanceStore, fakeBroker,
                tradeFeedCandleStore, Messenger.Default, parameterStore);
            var backTestRunner = new BackTestRunner(dynamicGraphs, picker, strategyInstanceStore,
                strategyRunner);
            var cancellationTokenSource = new CancellationTokenSource();

            var trades = player.ReadHistoricalData(currencyPair, fromDate.ToUniversalTime(), to.ToUniversalTime(),
                strategyInstance.PeriodSize, cancellationTokenSource.Token);
            // action

            var backTestResult = await backTestRunner.Run(strategyInstance, trades, CancellationToken.None);
            return backTestResult;
        }

        #region Setup/Teardown

        public void Setup()
        {
            //  TestLoggingHelper.EnsureExists();
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion


        [Test]
        [Timeout(240000)]
        [Explicit]
        public async Task ClearAllBackTests()
        {
            // arrange
            Setup();
            var context = await TestTradePersistenceFactory.RealDb().GetTradePersistence();
            var strategyInstances = context.Strategies.Where(x=>x.IsBackTest)
                .Include(x => x.Trades)
                .Include("Trades.Orders")
                .Include(x => x.Property).ToList();
            var refs = strategyInstances.Select(x=>x.Reference).ToArray();
            context.DynamicPlots.RemoveRange(context.DynamicPlots.Where(x=>refs.Contains(x.Feed)));
            context.TradeOrders.RemoveRange(strategyInstances.SelectMany(x=>x.Trades.OrEmpty()).SelectMany(r=>r.Orders.OrEmpty()));
            context.Trades.RemoveRange(strategyInstances.SelectMany(x => x.Trades));
            context.StrategyProperties.RemoveRange(strategyInstances.SelectMany(x=>x.Property));
            await context.SaveChangesAsync();
            context = await TestTradePersistenceFactory.RealDb().GetTradePersistence();
            strategyInstances = context.Strategies.Where(x => x.IsBackTest).ToList();
            context.Strategies.RemoveRange(strategyInstances);
            await context.SaveChangesAsync();
        }
    }
}