using System;
using SteveTheTradeBot.Api.AppStartup;
using SteveTheTradeBot.Api.Security;
using Bumbershoot.Utilities.Helpers;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Playground;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SteveTheTradeBot.Api.GraphQl
{
    public static class GraphQlSetup
    {
        public static void AddGraphQl(this IServiceCollection services)
        {
            services.AddInMemorySubscriptions();
            services.AddGraphQLServer()
                .AddQueryType<DefaultQuery>()
                .AddMutationType<DefaultMutation>()
                .AddSubscriptionType<DefaultSubscription>()
                .AddAuthorization();
        }

        public static void AddGraphQl(this IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.UseEndpoints(x => x.MapGraphQL());
            app.UsePlayground("/graphql", "/ui/playground");
        }
    }
}