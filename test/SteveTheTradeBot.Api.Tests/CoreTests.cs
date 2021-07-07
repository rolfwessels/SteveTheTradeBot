using SteveTheTradeBot.Api.Mappers;
using NUnit.Framework;

namespace SteveTheTradeBot.Api.Tests
{
    [TestFixture]
    public class CoreTests
    {
        [Test]
        public void AssertConfigurationIsValid_WhenCalled_ShouldBeValid()
        {
            MapApi.GetInstance();
            MapApi.AssertConfigurationIsValid();
        }
    }
}