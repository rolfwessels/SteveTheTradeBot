using System;
using SteveTheTradeBot.Api.AppStartup;
using SteveTheTradeBot.Dal.Models.Auth;
using Bumbershoot.Utilities.Helpers;
using IdentityModel;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SteveTheTradeBot.Api.Security
{
    public static class SecuritySetupClient
    {
        public static void AddBearerAuthentication(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddAuthorization(AddFromActivities);
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    var openIdSettings = IocApi.Instance.Resolve<OpenIdSettings>();
                    options.Authority = openIdSettings.HostUrl;
                    options.RequireHttpsMetadata = false;
                    options.ApiName = openIdSettings.ApiResourceName;
                    options.ApiSecret = openIdSettings.ApiResourceSecret;
                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromMinutes(5);
                });
        }

        public static void UseBearerAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();
        }

        #region Private Methods

        private static void AddFromActivities(AuthorizationOptions options)
        {
            EnumHelper.ToArray<Activity>()
                .ForEach(activity =>
                {
                    options.AddPolicy(UserClaimProvider.ToPolicyName(activity),
                        policyAdmin =>
                        {
                            policyAdmin.RequireClaim(JwtClaimTypes.Role, UserClaimProvider.ToPolicyName(activity));
                        });
                });
        }

        #endregion
    }
}