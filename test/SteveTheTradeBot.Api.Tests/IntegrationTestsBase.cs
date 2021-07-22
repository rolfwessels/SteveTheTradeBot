using System;
using SteveTheTradeBot.Dal.Tests;
using SteveTheTradeBot.Sdk;
using SteveTheTradeBot.Sdk.Helpers;
using SteveTheTradeBot.Sdk.RestApi;
using Serilog;
using Microsoft.Extensions.Hosting;

namespace SteveTheTradeBot.Api.Tests
{
    public class IntegrationTestsBase
    {
        public const string ClientId = "SteveTheTradeBot.Api";
        public const string AdminPassword = "admin!";
        public const string AdminUser = "admin@admin.com";
        protected static readonly Lazy<string> HostAddress;

        protected static Lazy<ConnectionFactory> _defaultRequestFactory;
        protected static Lazy<SteveTheTradeBotClient> _adminConnection;
        protected static Lazy<SteveTheTradeBotClient> _guestConnection;

        static IntegrationTestsBase()
        {
            RestSharpHelper.Log = Log.Debug;
            HostAddress = new Lazy<string>(StartHosting);
            _defaultRequestFactory = new Lazy<ConnectionFactory>(() => new ConnectionFactory(HostAddress.Value));
            _adminConnection = new Lazy<SteveTheTradeBotClient>(() => CreateLoggedInRequest(AdminUser, AdminPassword));
            _guestConnection = new Lazy<SteveTheTradeBotClient>(() => CreateLoggedInRequest("Guest@Guest.com", "guest!"));
        }

        public SteveTheTradeBotClient AdminClient()
        {
            return _adminConnection.Value;
        }

        public SteveTheTradeBotClient GuestClient()
        {
            return _guestConnection.Value;
        }

        #region Private Methods

        
        private static string StartHosting()
        {

            var port = new Random().Next(9500, 9699);
            var address = $"http://localhost:{port}";
            Environment.SetEnvironmentVariable("OpenId__HostUrl", address);
            Environment.SetEnvironmentVariable("OpenId__UseReferenceTokens", "true"); //helps with testing on appveyor
            TestLoggingHelper.EnsureExists();

            var host = Program.BuildWebHost(address);
            host.RunAsync().ConfigureAwait(false);

            Log.Information($"Starting api on [{address}]");
            var forContext = Log.ForContext(typeof(RestSharpHelper));
            RestSharpHelper.Log = m => { forContext.Debug(m); };
            return address;
        }


        private static SteveTheTradeBotClient CreateLoggedInRequest(string adminAdminCom, string adminPassword)
        {
            var steveTheTradeBotApi = _defaultRequestFactory.Value.GetConnection();
            steveTheTradeBotApi.Authenticate.Login(adminAdminCom, adminPassword).Wait();
            return (SteveTheTradeBotClient) steveTheTradeBotApi;
        }

        protected SteveTheTradeBotClient NewClientNotAuthorized()
        {
            return (SteveTheTradeBotClient) _defaultRequestFactory.Value.GetConnection();
        }

        #endregion
    }
}