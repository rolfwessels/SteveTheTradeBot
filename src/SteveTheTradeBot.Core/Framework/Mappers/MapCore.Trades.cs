using AutoMapper;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using SteveTheTradeBot.Dal.Models.Trades;

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
    }
}