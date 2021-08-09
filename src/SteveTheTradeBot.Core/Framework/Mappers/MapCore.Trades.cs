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
            cfg.CreateMap<TradeQuote, TradeQuote>()
                .ForMember(x => x.CreateDate, opt => opt.Ignore())
                .ForMember(x => x.UpdateDate, opt => opt.Ignore());
            cfg.CreateMap<TradeResponseDto, HistoricalTrade>()
                .ForMember(x => x.CreateDate, opt => opt.Ignore())
                .ForMember(x => x.UpdateDate, opt => opt.Ignore());
            cfg.CreateMap<HistoricalTrade, TradeResponseDto>();
        }

        public static TradeQuote CopyValuesTo(this TradeQuote fromValue, TradeQuote to)
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

        public static ModelInput ToModelInput(this TradeQuote tradeQuote)
        {
            static float ToFloat(decimal? @decimal)
            {
                if (@decimal.HasValue) return (float)@decimal;
                return 0;
            }

            var sampleData = new ModelInput()
            {
                Close = (float)tradeQuote.Close,
                Volume = (float)tradeQuote.Volume,
                Macd = ToFloat(tradeQuote.Metric["macd"]),
                Rsi14 = ToFloat(tradeQuote.Metric["rsi14"]),
                Ema100 = ToFloat(tradeQuote.Metric["ema100"]),
                Ema200 = ToFloat(tradeQuote.Metric["ema200"]),
                Roc100 = ToFloat(tradeQuote.Metric["roc100"]),
                Roc200 = ToFloat(tradeQuote.Metric["roc200"]),
                Roc100sma = ToFloat(tradeQuote.Metric["roc100-sma"]),
                Roc200sma = ToFloat(tradeQuote.Metric["roc200-sma"]),
                Supertrend = ToFloat(tradeQuote.Metric["supertrend"]),
                Macdsignal = ToFloat(tradeQuote.Metric["macd-signal"]),
                Macdhistogram = ToFloat(tradeQuote.Metric["macd-histogram"]),
                Supertrendlower = ToFloat(tradeQuote.Metric["supertrend-lower"]),
                Supertrendupper = ToFloat(tradeQuote.Metric["supertrend-upper"]),
            };
            return sampleData;
        }
    }
}