using System;
using System.Collections.Generic;
using System.Linq;

namespace SteveTheTradeBot.Api.WebApi.Controllers
{
    public interface IQueryableControllerBase<TDal, TModel>
    {
        List<TModel> Query(Func<IQueryable<TDal>, IQueryable<TDal>> apply);
        int Count(Func<IQueryable<TDal>, IQueryable<TDal>> apply);
    }
}