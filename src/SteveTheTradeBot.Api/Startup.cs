using System;
using System.Reflection;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using SteveTheTradeBot.Api.AppStartup;
using SteveTheTradeBot.Api.GraphQl;
using SteveTheTradeBot.Api.Security;
using SteveTheTradeBot.Api.Swagger;
using SteveTheTradeBot.Api.WebApi.Filters;
using SteveTheTradeBot.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty;

namespace SteveTheTradeBot.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Settings.Initialize(Configuration);
            Redis = ConnectionMultiplexer.Connect(Settings.Instance.RedisHost);
        }

        public ConnectionMultiplexer Redis { get; set; }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            IocApi.Populate(services);
            services.AddSingleton(x => TradePersistenceFactory.DbContextOptions(Settings.Instance.NpgsqlConnection));
            services.AddDbContext<TradePersistenceStoreContext>();
            services.AddCors();
            services.AddGraphQl();
            services.UseIdentityService(Configuration);
            services.AddBearerAuthentication();
            services.AddMvc(config => { config.Filters.Add(new CaptureExceptionFilter()); });
            services.AddSwagger();
            services.AddSignalR();
            services.AddHangfire(configuration =>
            {
                configuration.UseRedisStorage(Redis);
                //RecurringJob.AddOrUpdate<IUpdateHistoricalData>("refresh", x => x.StartUpdate("BTCZAR"), Cron.Daily);
            });
            return new AutofacServiceProvider(IocApi.Instance.Container);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            app.UseStaticFiles();
            app.UseRouting();
            var openIdSettings = new OpenIdSettings(Configuration);

            app.UseCors(policy =>
            {
                policy.AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10)) // Cache the OPTIONS calls.
                    .WithOrigins(openIdSettings.GetOriginList());
            });

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseIdentityService(openIdSettings);
            app.UseBearerAuthentication();
            app.UseAuthentication();
            app.UseAuthorization();
            app.AddGraphQl();
            app.UseEndpoints(e => e.MapControllers());
            app.UseSwagger();
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            SimpleFileServer.Initialize(app);
        }

        public static string InformationalVersion()
        {
            return Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
        }
    }
}