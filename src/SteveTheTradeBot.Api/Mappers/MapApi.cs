using System;
using AutoMapper;
using SteveTheTradeBot.Core.Framework.MessageUtil.Models;
using SteveTheTradeBot.Shared.Models.Shared;

namespace SteveTheTradeBot.Api.Mappers
{
    public static partial class MapApi
    {
        private static readonly Lazy<IMapper> _mapper;

        static MapApi()
        {
            _mapper = new Lazy<IMapper>(InitializeMapping);
        }

        public static IMapper GetInstance()
        {
            return _mapper.Value;
        }


        public static ValueUpdateModel<TModel> ToValueUpdateModel<T, TModel>(this DalUpdateMessage<T> updateMessage)
        {
            return new ValueUpdateModel<TModel>(GetInstance().Map<T, TModel>(updateMessage.Value),
                (UpdateTypeCodes) updateMessage.UpdateType);
        }

        public static void AssertConfigurationIsValid()
        {
            GetInstance().ConfigurationProvider.AssertConfigurationIsValid();
        }

        #region Private Methods

        private static IMapper InitializeMapping()
        {
            var config = new MapperConfiguration(cfg =>
            {
                MapUserModel(cfg);
                MapProjectModel(cfg);
                MapUserGrantModel(cfg);
            });
            return config.CreateMapper();
        }

        #endregion
    }
}