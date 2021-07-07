using System;
using System.Collections.Generic;
using System.Linq;
using SteveTheTradeBot.Dal.Models.Base;
using SteveTheTradeBot.Dal.Persistence;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Dal.Tests;
using FizzWare.NBuilder;

namespace SteveTheTradeBot.Core.Tests.Helpers
{
    public static class FakeRepoHelper
    {
        public static IList<T> AddFake<T>(this IRepository<T> repository, int size, Action<T> applyUpdate)
            where T : IBaseDalModel
        {
            var items = Builder<T>.CreateListOfSize(size).WithValidData().Build();
            items.OfType<IBaseDalModelWithId>().ForEach(x => x.Id = null);
            return items
                .ForEach(applyUpdate)
                .Select(repository.Add)
                .Select(x => x.Result)
                .ToList();
        }

        public static IList<T> AddFake<T>(this IRepository<T> repository, int size = 5) where T : IBaseDalModel
        {
            return AddFake(repository, size, t => { });
        }

        public static T AddAFake<T>(this IRepository<T> repository) where T : IBaseDalModel
        {
            return AddFake(repository, 1).FirstOrDefault();
        }


        public static T AddAFake<T>(this IRepository<T> repository, Action<T> applyUpdate) where T : IBaseDalModel
        {
            return AddFake(repository, 1, applyUpdate).FirstOrDefault();
        }
    }
}