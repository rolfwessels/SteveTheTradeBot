using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Storage;

namespace SteveTheTradeBot.Core.Tests.Components.Storage
{
    [TestFixture]
    public class TradePersistenceFactoryTests
    {
        [Test]
        public async Task GetConnection_GivenConnectionDetails_ShouldReturnSingleInstance()
        {
            // arrange
            var factory = new TradePersistenceFactory(Settings.Instance.NpgsqlConnection);
            // action
            var persistence = await factory.GetTradePersistence();
            // assert
            persistence.Should().NotBe(null);
        }

       
    }
}