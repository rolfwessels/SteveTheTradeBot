using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Dal.Models.Users;
using FluentAssertions;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests.Components.Users
{
    [TestFixture]
    public class GravatarHelperTests
    {
        [Test]
        public void BuildUrl_GivenUpperCaseValue_ShouldToLower()
        {
            // arrange
            var value = "MyEmailAddress@example.com";
            // action
            var buildUrl = GravatarHelper.BuildUrl(value);
            // assert
            buildUrl.Should().Contain("0bc83cb571cd1c50ba6f3e8a78ef1346");
        }

        [Test]
        public void BuildUrl_GivenTrim_ShouldToLower()
        {
            // arrange
            var value = "MyEmailAddress@example.com ";
            // action
            var buildUrl = GravatarHelper.BuildUrl(value);
            // assert
            buildUrl.Should().Contain("0bc83cb571cd1c50ba6f3e8a78ef1346");
        }


        [Test]
        public void BuildUrl_GivenMe_ShouldHaveCorrectUrl()
        {
            // arrange
            var value = "Rolf.wessels@gmail.com";
            // action
            var buildUrl = GravatarHelper.BuildUrl(value);
            // assert
            buildUrl.Should().Contain("https://www.gravatar.com/avatar/1b2014523c03b4dbe0fd0211850cfbaf?d=robohash");
        }
    }
}