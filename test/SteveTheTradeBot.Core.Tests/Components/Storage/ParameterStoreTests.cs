using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Storage;

namespace SteveTheTradeBot.Core.Tests.Components.Storage
{
    public class ParameterStoreTests
    {
        private ParameterStore _parameterStore;

        [Test]
        public async Task Set_GivenAValue_ShouldStoreResult()
        {
            // arrange
            Setup();
            // action
            await _parameterStore.Set("test", "12312");
            var value = await _parameterStore.Get("test", "");
            // assert
            value.Should().Be("12312");
        }

        [Test]
        public async Task Get_GivenNoValue_ShouldReturnDefault()
        {
            // arrange
            Setup();
            // action
            var value =  await _parameterStore.Get("test", "def");
            // assert
            value.Should().Be("def");
        }

        private void Setup()
        {
            _parameterStore = new ParameterStore(TestTradePersistenceFactory.UniqueDb());
        }
    }
}