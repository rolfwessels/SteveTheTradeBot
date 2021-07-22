using System.Linq;
using System.Threading.Tasks;
using SteveTheTradeBot.Sdk.RestApi;
using Bumbershoot.Utilities.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace SteveTheTradeBot.Api.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class AuthenticateApiClientTests : IntegrationTestsBase
    {
        private ISteveTheTradeBotClient _connection;
        private ISteveTheTradeBotClient _connectionAuth;


        #region Setup/Teardown

        protected void Setup()
        {
            _connection = _defaultRequestFactory.Value.GetConnection();
            _connectionAuth = _defaultRequestFactory.Value.GetConnection();   
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion

        [Test]
        public async Task AfterLogin_WhenUsingApi_ShouldGetResults()
        {
            // arrange
            Setup();
            var pingModel = await _connection.Ping.Get();
            // action
            var data = await _connectionAuth.Authenticate.Login(AdminUser, AdminPassword);
            _connection.SetToken(data);
            var projectsEnumerable = await _connection.Projects.All();
            // assert
            pingModel.Environment.ToLower().Should().NotBeEmpty();
            projectsEnumerable.Count().Should().BeGreaterThan(0);
        }

        [Test]
        public async Task CheckForWellKnowConfig_WhenCalled_ShouldHaveResult()
        {
            // arrange
            Setup();
            // action
            var data = await _connection.Authenticate.GetConfigAsync();
            // assert
            data.Keys.Dump("data.Keys");
            data.Keys.First().Keys.Should().Contain("kty");
        }

        [Test]
        public async Task GivenAuthorization_WhenCalled_ShouldHaveResult()
        {
            // arrange
            Setup();
            // action
            var auth = await _connection.Authenticate.Login(AdminUser, AdminPassword);
            // assert
            auth.AccessToken.Should().NotBeEmpty();
            auth.ExpiresIn.Should().BeGreaterThan(30);
            auth.TokenType.Should().Be("Bearer");
        }
    }
}