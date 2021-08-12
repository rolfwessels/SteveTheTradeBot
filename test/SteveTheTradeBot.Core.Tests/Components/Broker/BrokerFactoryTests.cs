using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;

namespace SteveTheTradeBot.Core.Tests.Components.Broker
{
    public class BrokerFactoryTests
    {
        [Test]
        public void GetBroker_GivenValrBrokerPaperTradingApi_ShouldReturnValrBrokerApi()
        {
            // arrange
            var keyValuePairs = new Dictionary<string, string> {
                {"Valr:EncryptionKey", "ValrBrokerApi"},
                {"Valr:ApiName", "ValrBrokerPaperTradingApi"}
            };
            var configurationRoot = new ConfigurationBuilder()
                .AddInMemoryCollection(keyValuePairs).Build();
            var settings = new ValrSettings(configurationRoot);
            // action
            var brokerApi = new BrokerFactory(settings).GetBroker();
            // assert
            brokerApi.GetType().Name.Should().Be("ValrBrokerPaperTradingApi");
        }


        [Test]
        public void GetBroker_GivenValrBrokerApi_ShouldReturnValrBrokerApi()
        {
            // arrange
            var keyValuePairs = new Dictionary<string, string> {
                {"Valr:EncryptionKey", "ValrBrokerApi"},
                {"Valr:ApiName", "ValrBrokerApi"}
            };
            var configurationRoot = new ConfigurationBuilder()
                .AddInMemoryCollection(keyValuePairs).Build();
            var settings = new ValrSettings(configurationRoot);
            // action
            var brokerApi = new BrokerFactory(settings).GetBroker();
            // assert
            brokerApi.GetType().Name.Should().Be("ValrBrokerApi");
        }

        [Test]
        public void GetBroker_GivenAnythingElse_ShouldReturnValrBrokerApi()
        {
            // arrange
            var keyValuePairs = new Dictionary<string, string> {
                {"Valr:EncryptionKey", "ValrBrokerApi"},
                {"Valr:ApiName", "AnythingElse"}
            };
            var configurationRoot = new ConfigurationBuilder()
                .AddInMemoryCollection(keyValuePairs).Build();
            var settings = new ValrSettings(configurationRoot);
            // action
            var brokerApi = new BrokerFactory(settings).GetBroker();
            // assert
            brokerApi.GetType().Name.Should().Be("ValrBrokerPaperTradingApi");
        }
    }
}