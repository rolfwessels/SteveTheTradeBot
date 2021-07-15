using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;

namespace SteveTheTradeBot.Core.Tests.Components.ThirdParty.Valr
{
    [Category("API")]
    public class ValrBrokerApiTests
    {
        private ValrBrokerApi _api;

        [Test]
        public void method_GiventestingFor_Shouldresult()
        {
            // arrange
            Setup();
            ValrSettings.Instance.ApiKey.Should().NotEndWith("...");
            ValrSettings.Instance.ApiKey.Should().NotEndWith("...");
            // action

            // assert
        }

        [Test]
        public void SignBody_GivenSetValues_ShouldSignCorrectly()
        {
            // arrange
            Setup();
            _api = new ValrBrokerApi(ValrSettings.Instance.ApiKey, "4961b74efac86b25cce8fbe4c9811c4c7a787b7a5996660afcc2e287ad864363");
            // action
            var signBody = _api.SignBody("1558014486185", "GET", "/v1/account/balances");
            // assert
            signBody.Should().Be("9d52c181ed69460b49307b7891f04658e938b21181173844b5018b2fe783a6d4c62b8e67a03de4d099e7437ebfabe12c56233b73c6a0cc0f7ae87e05f6289928");
        }

        [Test]
        public void SignBody_GivenSetValuesAndBody_ShouldSignCorrectly()
        {
            // arrange
            Setup();
            _api = new ValrBrokerApi(ValrSettings.Instance.ApiKey, "4961b74efac86b25cce8fbe4c9811c4c7a787b7a5996660afcc2e287ad864363");
            // action
            var signBody = _api.SignBody("1558017528946", "POST", "/v1/orders/market", @"{""customerOrderId"":""ORDER-000001"",""pair"":""BTCZAR"",""side"":""BUY"",""quoteAmount"":""80000""}");
            // assert
            signBody.Should().Be("be97d4cd9077a9eea7c4e199ddcfd87408cb638f2ec2f7f74dd44aef70a49fdc49960fd5de9b8b2845dc4a38b4fc7e56ef08f042a3c78a3af9aed23ca80822e8");
        }

        private void Setup()
        {
            _api = new ValrBrokerApi(ValrSettings.Instance.ApiKey, ValrSettings.Instance.Secret);
        }



    }
}