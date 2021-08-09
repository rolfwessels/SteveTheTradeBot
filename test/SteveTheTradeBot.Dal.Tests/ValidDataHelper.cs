using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Dal.Models.Projects;
using SteveTheTradeBot.Dal.Models.Users;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Dal.Tests
{
    public static class ValidDataHelper
    {
        private static int _counter = 0;
        public static Random _random = new Random();
        public static ISingleObjectBuilder<T> WithValidData<T>(this ISingleObjectBuilder<T> value)
        {
            return value.With(ValidData);
        }

        public static IListBuilder<T> WithValidData<T>(this IListBuilder<T> value)
        {
            return value.All().With(ValidData);
        }

        #region Private Methods

        private static T ValidData<T>(T value)
        {
            if (value is Project project)
                project.Name = GetRandom.String(20);

            if (value is User user)
            {
                user.Name = GetRandom.FirstName() + " " + GetRandom.LastName();
                user.Email = (Regex.Replace(user.Name.ToLower(), "[^a-z]", "") + GetRandom.NumericString(3) +
                              "@nomailmail.com").ToLower();
                user.HashedPassword = GetRandom.String(20);
                user.Roles.Add("Guest");
            }

            if (value is HistoricalTrade historicalTrade)
            {
                historicalTrade.Price = 259653+ _random.Next(-1000, 1000);
                historicalTrade.Quantity = _random.Next(1, 1000)/1000m;
                historicalTrade.CurrencyPair = "BTCZAR";
                historicalTrade.TradedAt = DateTime.Now.AddMinutes(_counter*-1).ToUniversalTime();
                historicalTrade.TakerSide = _random.Next(0,1) == 1 ? "buy" : "sell";
                historicalTrade.SequenceId = _counter;
                historicalTrade.Id = Guid.NewGuid().ToString();
                historicalTrade.QuoteVolume = 259.653m + _random.Next(-20, 20);
            }
            
            if (value is TradeQuote tradeFeedCandle)
            {
              
                tradeFeedCandle.Volume = 259 + _random.Next(-1000, 1000);
                tradeFeedCandle.Open = 259653+ _random.Next(-1000, 1000);
                tradeFeedCandle.Close = 259653 + _random.Next(-1000, 1000);
                tradeFeedCandle.High = Math.Max(tradeFeedCandle.Open, tradeFeedCandle.Close);
                tradeFeedCandle.Low = Math.Min(tradeFeedCandle.Open, tradeFeedCandle.Close);
                tradeFeedCandle.PeriodSize = PeriodSize.OneMinute;
                tradeFeedCandle.Date = DateTime.Now.AddMinutes(_counter * -1).ToUniversalTime();
                tradeFeedCandle.Feed = "feed1";
                tradeFeedCandle.Metric = new Dictionary<string, decimal?>() { { "ema", 12} , { "rsa", 3} };
            }

            if (value is StrategyInstance instance)
            {
               
                instance.Feed = "test";
                instance.Pair= new[] { CurrencyPair.BTCZAR , CurrencyPair.ETHZAR , CurrencyPair.XRPZAR}[_counter%3] ;
                instance.IsActive = true;
                instance.IsBackTest = false;
                instance.InvestmentAmount = new[] {500, 1000, 1500}[_counter % 3];
                instance.QuoteAmount = instance.InvestmentAmount * 1.1m;
                instance.Recalculate();
            }

            _counter++;
            var userGrant = value as UserGrant;
            if (userGrant != null) userGrant.User = Builder<User>.CreateNew().Build().ToReference();
            return value;
        }

        #endregion
    }
}