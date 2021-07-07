using System;
using AutoMapper;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Core.Framework.Mappers
{
    public static partial class MapCore
    {
        private static readonly Lazy<IMapper> _mapper = new Lazy<IMapper>(InitializeMapping);

        public static IMapper GetInstance()
        {
            return _mapper.Value;
        }

        private static IMapper InitializeMapping()
        {
            var config = new MapperConfiguration(cfg =>
            {
                CreateCommandMap(cfg);
                CreateProjectMap(cfg);
                CreateUserMap(cfg);
            });
            return config.CreateMapper();
        }

        public static IMappingExpression<T, T2> IgnoreCreateUpdate<T, T2>(
            this IMappingExpression<T, T2> mappingExpression) where T2 : BaseDalModel
        {
            return mappingExpression
                .ForMember(x => x.CreateDate, opt => opt.Ignore())
                .ForMember(x => x.UpdateDate, opt => opt.Ignore());
        }

        public static void AssertConfigurationIsValid()
        {
            GetInstance().ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}