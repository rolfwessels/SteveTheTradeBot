using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Dal.Models.Auth;
using FluentAssertions;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests.Components.Users
{
    [TestFixture]
    public class RoleManagerTests
    {
        private RoleManager _roleManager;

        #region Setup/Teardown

        public void Setup()
        {
            _roleManager = new RoleManager();
        }

        #endregion

        [Test]
        public void GetRoleByName_GivenAdminRole_ShouldReturn()
        {
            // arrange
            Setup();
            // action
            var roleByName = _roleManager.GetRoleByName("Admin").Result;
            // assert
            roleByName.Name.Should().Be("Admin");
            roleByName.Activities.Should().Contain(Activity.DeleteUser);
            roleByName.Activities.Should().NotBeEmpty();
        }

        [Test]
        public void GetRoleByName_GivenGuestRole_ShouldReturn()
        {
            // arrange
            Setup();
            // action
            var roleByName = _roleManager.GetRoleByName("Guest").Result;
            // assert
            roleByName.Name.Should().Be("Guest");
            roleByName.Activities.Should().NotContain(Activity.DeleteUser);
            roleByName.Activities.Should().NotBeEmpty();
        }

        [Test]
        public void GetRoleByName_GivenInvalidRole_ShouldReturnNull()
        {
            // arrange
            Setup();
            // action
            var roleByName = _roleManager.GetRoleByName("Guest123123").Result;
            // assert
            roleByName.Should().BeNull();
        }
    }
}