using System;
using Skender.Stock.Indicators;

namespace SteveTheTradeBot.Core.Utils
{
    public static class PeriodSizeHelper
    {
        public static TimeSpan ToTimeSpan(this PeriodSize periodSize)
        {
            switch (periodSize)
            {
                case PeriodSize.Month:
                    return TimeSpan.FromDays(31);
                    
                case PeriodSize.Week:
                    return TimeSpan.FromDays(7);
                    
                case PeriodSize.Day:
                    return TimeSpan.FromDays(1);
                    
                case PeriodSize.FourHours:
                    return TimeSpan.FromHours(4);
                    
                case PeriodSize.TwoHours:
                    return TimeSpan.FromHours(2);
                    
                case PeriodSize.OneHour:
                    return TimeSpan.FromHours(1);
                    
                case PeriodSize.ThirtyMinutes:
                    return TimeSpan.FromMinutes(30);
                    
                case PeriodSize.FifteenMinutes:
                    return TimeSpan.FromMinutes(15);
                    
                case PeriodSize.FiveMinutes:
                    return TimeSpan.FromMinutes(5);
                    
                case PeriodSize.ThreeMinutes:
                    return TimeSpan.FromMinutes(3);
                    
                case PeriodSize.TwoMinutes:
                    return TimeSpan.FromMinutes(2);
                    
                case PeriodSize.OneMinute:
                    return TimeSpan.FromMinutes(1);
                    
                default:
                    throw new ArgumentOutOfRangeException(nameof(periodSize), periodSize, null);
            }
        }

        public static TimeSpan ToObserverPeriod(this PeriodSize periodSize)
        {
            switch (periodSize)
            {
                case PeriodSize.Month:
                    return TimeSpan.FromDays(31*3);

                case PeriodSize.Week:
                    return TimeSpan.FromDays(7*4);

                case PeriodSize.Day:
                    return TimeSpan.FromDays(7);

                case PeriodSize.FourHours:
                    return TimeSpan.FromDays(1); ;

                case PeriodSize.TwoHours:
                    return TimeSpan.FromHours(12); ;

                case PeriodSize.OneHour:
                    return TimeSpan.FromHours(8);

                case PeriodSize.ThirtyMinutes:
                    return TimeSpan.FromHours(4);

                case PeriodSize.FifteenMinutes:
                    return TimeSpan.FromHours(2);

                case PeriodSize.FiveMinutes:
                    return TimeSpan.FromHours(1);

                case PeriodSize.ThreeMinutes:
                    return TimeSpan.FromHours(1);

                case PeriodSize.TwoMinutes:
                    return TimeSpan.FromHours(1);

                case PeriodSize.OneMinute:
                    return TimeSpan.FromHours(1);

                default:
                    throw new ArgumentOutOfRangeException(nameof(periodSize), periodSize, null);
            }
        }
    }
}