using AutoMapper;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Dal.Models.Trades;
using SteveTheTradeBotML.Model;

namespace SteveTheTradeBot.Core.Framework.Mappers
{
    public static partial class MapCore
    {
        public static void CreateTradesMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<TradeFeedCandle, TradeFeedCandle>()
                .ForMember(x => x.CreateDate, opt => opt.Ignore())
                .ForMember(x => x.UpdateDate, opt => opt.Ignore());
            cfg.CreateMap<TradeResponseDto, HistoricalTrade>()
                .ForMember(x => x.CreateDate, opt => opt.Ignore())
                .ForMember(x => x.UpdateDate, opt => opt.Ignore());
            cfg.CreateMap<HistoricalTrade, TradeResponseDto>();
        }

        public static TradeFeedCandle CopyValuesTo(this TradeFeedCandle fromValue, TradeFeedCandle to)
        {
            return GetInstance().Map(fromValue, to);
        }

        public static HistoricalTrade ToDao(this TradeResponseDto project, HistoricalTrade projectReference = null)
        {
            return GetInstance().Map(project, projectReference);
        }

        public static TradeResponseDto ToDto(this HistoricalTrade project, TradeResponseDto projectReference = null)
        {
            return GetInstance().Map(project, projectReference);
        }

        public static ModelInput ToModelInput(this TradeFeedCandle tradeFeedCandle)
        {
            static float ToFloat(decimal? @decimal)
            {
                if (@decimal.HasValue) return (float)@decimal;
                return 0;
            }

            var sampleData = new ModelInput()
            {
                Close = (float)tradeFeedCandle.Close,
                Volume = (float)tradeFeedCandle.Volume,
                Macd = ToFloat(tradeFeedCandle.Metric["macd"]),
                Rsi14 = ToFloat(tradeFeedCandle.Metric["rsi14"]),
                Ema100 = ToFloat(tradeFeedCandle.Metric["ema100"]),
                Ema200 = ToFloat(tradeFeedCandle.Metric["ema200"]),
                Roc100 = ToFloat(tradeFeedCandle.Metric["roc100"]),
                Roc200 = ToFloat(tradeFeedCandle.Metric["roc200"]),
                Roc100sma = ToFloat(tradeFeedCandle.Metric["roc100-sma"]),
                Roc200sma = ToFloat(tradeFeedCandle.Metric["roc200-sma"]),
                Supertrend = ToFloat(tradeFeedCandle.Metric["supertrend"]),
                Macdsignal = ToFloat(tradeFeedCandle.Metric["macd-signal"]),
                Macdhistogram = ToFloat(tradeFeedCandle.Metric["macd-histogram"]),
                Supertrendlower = ToFloat(tradeFeedCandle.Metric["supertrend-lower"]),
                Supertrendupper = ToFloat(tradeFeedCandle.Metric["supertrend-upper"]),
            };
            return sampleData;
        }
    }
}