using SteveTheTradeBot.Api.Mappers;
using SteveTheTradeBot.Core.Framework.Mappers;
using NUnit.Framework;

namespace SteveTheTradeBot.Api.Tests.Mappers
{
    [TestFixture]
    public class AutoMapperSetupTests
    {
        [Test]
        public void AssertConfigurationIsValid_OnMapApi_ShouldNotFail()
        {
            // assert
            MapApi.AssertConfigurationIsValid();
        }

        [Test]
        public void AssertConfigurationIsValid_OnMapCore_ShouldNotFail()
        {
            // assert
            MapCore.AssertConfigurationIsValid();
        }
    }
}