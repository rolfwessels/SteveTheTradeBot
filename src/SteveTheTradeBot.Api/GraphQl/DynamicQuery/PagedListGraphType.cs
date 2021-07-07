using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Dal.Models.Auth;
using HotChocolate.Types;

namespace SteveTheTradeBot.Api.GraphQl.DynamicQuery
{
    public class PagedListGraphType<TDal, TGt> : ObjectType<PagedList<TDal>> where TGt : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor<PagedList<TDal>> descriptor)
        {
            Name = $"{typeof(TDal).Name}PagedList";
            descriptor.Field("items")
                .Description("All items paged.")
                .Type<NonNullType<ListType<TGt>> >()
                .Resolver(x => x.Parent<PagedList<TDal>>().Items)
                .RequirePermission(Activity.ReadProject);

            descriptor.Field("count")
                .Description("The total item count.")
                .Type<NonNullType<LongType>>()
                .Resolver(x => x.Parent<PagedList<TDal>>().Count)
                .RequirePermission(Activity.ReadProject);
        }
    }
}