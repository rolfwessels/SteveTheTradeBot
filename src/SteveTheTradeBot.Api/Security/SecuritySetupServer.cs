using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Bumbershoot.Utilities.Helpers;
using IdentityServer4.Extensions;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;

namespace SteveTheTradeBot.Api.Security
{
    public static class SecuritySetupServer
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);


        public static void UseIdentityService(this IServiceCollection services, IConfiguration configuration)
        {
            
            var openIdSettings = new OpenIdSettings(configuration);
            _log.Debug($"SecuritySetupServer:UseIdentityService Setting the host url {openIdSettings.HostUrl}");
            services.AddIdentityServer()
                .AddSigningCredential(Certificate(openIdSettings.CertPfx, openIdSettings.CertPassword, openIdSettings.CertStoreThumbprint))
                .AddInMemoryIdentityResources(OpenIdConfig.GetIdentityResources())
                .AddInMemoryApiScopes(OpenIdConfig.GetApiScopes(openIdSettings))
                .AddInMemoryApiResources(OpenIdConfig.GetApiResources(openIdSettings))
                .AddInMemoryClients(OpenIdConfig.GetClients(openIdSettings))
                .Services
                    .AddTransient<IPersistedGrantStore, PersistedGrantStore>()
                    .AddTransient<IResourceOwnerPasswordValidator, UserClaimProvider>();

            if (openIdSettings.IsDebugEnabled)
            {
                IdentityModelEventSource.ShowPII = true;
            }
        }

        public static void UseIdentityService(this IApplicationBuilder app, OpenIdSettings openIdSettings)
        {
            // update the host url if required
            // https://github.com/IdentityServer/IdentityServer4/issues/4592#issuecomment-659115122
            app.Use(async (context, next) =>
            {
                if (openIdSettings.HostUrl.StartsWith("https"))
                {
                    context.SetIdentityServerOrigin(openIdSettings.HostUrl);
                    context.SetIdentityServerBasePath(context.Request.PathBase.Value.TrimEnd('/'));
                }
                await next.Invoke();
            });
            app.UseIdentityServer();
        }

        #region Private Methods

        private static X509Certificate2 Certificate(string certFile, string password, string certStoreThumbprint)
        {
            try
            {
                X509Certificate2 cert = null;
                if (!string.IsNullOrEmpty(certStoreThumbprint)) cert = LoadCertFromStore(certStoreThumbprint);
                return cert ?? LoadCertFromFile(certFile, password);
            }
            catch (Exception e)
            {
                _log.Error($"SecuritySetupServer:Certificate {e.Message}");
                throw;
            }
        }

        private static X509Certificate2 LoadCertFromStore(string certStoreThumbprint)
        {
            using var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);
            var certCollection = certStore.Certificates.Find(
                X509FindType.FindByThumbprint,
                certStoreThumbprint,
                false);
            // Get the first cert with the thumbprint
            if (certCollection.Count <= 0) return null;
            _log.Information($"Successfully loaded cert from registry: {certCollection[0].Thumbprint}");
            return certCollection[0];
        }

        private static X509Certificate2 LoadCertFromFile(string certFile, string password)
        {
            var fileName = Path.Combine("./Certificates", certFile);
            if (!File.Exists(fileName))
            {
                _log.Error(
                    $"SecuritySetupServer:Certificate Could not load file {Path.GetFullPath(fileName)} to obtain the certificate.");
            }
            else
            {
               var cert = new X509Certificate2(fileName, password);
                _log.Information($"Falling back to cert from file. Successfully loaded: {cert.Thumbprint}");
                return cert;

            }
            return null;
        }

        #endregion
    }
}