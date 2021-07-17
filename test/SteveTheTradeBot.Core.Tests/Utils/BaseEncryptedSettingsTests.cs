using System.Collections.Generic;
using Bumbershoot.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SteveTheTradeBot.Core.Utils;
using SteveTheTradeBot.Dal.Tests;

namespace SteveTheTradeBot.Core.Tests.Utils
{
    [TestFixture]
    public class BaseEncryptedSettingsTests
    {
        private const string _key = "b14ca5898a4e4133bbce2ea2315a1916";
        private SampleSettings _sampleSettings;

        [Test]
        public void ReadEncryptedValue_GivenNoEncryptedValue_ShouldReturnDefault()
        {
            // arrange
            Setup();
            // action
            var value = _sampleSettings.ReadEncryptedValue("one", "no");
            // assert
            value.Should().Be("two");
        }

        [Test]
        public void ReadEncryptedValue_GivenInvalidEncryptedValue_ShouldReturnDefaultIfDecryptionFails()
        {
            // arrange
            Setup();
            // action
            var value = _sampleSettings.ReadEncryptedValue("two", "no");
            // assert
            value.Should().Be("no");
        }

        [Test]
        public void ReadEncryptedValue_GivenValidEncryptedValue_ShouldReturnValue()
        {
            // arrange
            Setup();
            var expected = "yar";
            // action
            var encryptString = _sampleSettings.EncryptString(expected);
            var value = _sampleSettings.ReadEncryptedValue("three",encryptString );
            // assert
            value.Should().Be(expected);
        }

        private void Setup()
        {
            TestLoggingHelper.EnsureExists();
            var keyValuePairs = new Dictionary<string, string> {
                {"one", "two"},
                {"two", "ENC:123"},
                {"EncryptionKey", _key}
            };
            _sampleSettings = new SampleSettings(new ConfigurationBuilder()
                .AddInMemoryCollection(keyValuePairs).Build());
        }

        private class SampleSettings : BaseEncryptedSettings
        {
            public SampleSettings(IConfiguration configuration) : base(configuration, "")
            {
            }

        }
    }
}
