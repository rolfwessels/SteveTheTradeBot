using System;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using SteveTheTradeBot.Api.AppStartup;
using SteveTheTradeBot.Api.GraphQl;
using SteveTheTradeBot.Api.Security;
using SteveTheTradeBot.Api.Swagger;
using SteveTheTradeBot.Api.WebApi.Filters;
using SteveTheTradeBot.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using SteveTheTradeBot.Core.Components.Storage;

namespace SteveTheTradeBot.Api
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            IocApi.Instance.SetBuilder(builder);
        }

        public ConnectionMultiplexer Redis { get; set; }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Settings.Instance.DataProtectionFolder))
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });
            services.AddSingleton(x => TradePersistenceFactory.DbContextOptions(Settings.Instance.NpgsqlConnection));
            services.AddDbContext<TradePersistenceStoreContext>();
            services.AddCors();
            services.AddGraphQl();
            services.UseIdentityService(Configuration);
            services.AddBearerAuthentication();
            services.AddMvc(config => { config.Filters.Add(new CaptureExceptionFilter()); });
            services.AddSwagger();
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            IocApi.Instance.SetContainer(app.ApplicationServices.GetAutofacRoot());

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