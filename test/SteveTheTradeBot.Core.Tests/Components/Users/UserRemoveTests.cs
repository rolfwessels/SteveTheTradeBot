using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Core.Tests.Framework.BaseManagers;
using SteveTheTradeBot.Core.Tests.Helpers;
using SteveTheTradeBot.Dal.Models.Users;
using SteveTheTradeBot.Dal.Persistence;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Dal.Tests;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests.Components.Users
{
    [TestFixture]
    public class UserRemoveTests : BaseManagerTests
    {
        private UserRemove.Handler _handler;
        private IRepository<User> _users;

        #region Setup/Teardown

        public override void Setup()
        {
            base.Setup();
            _handler = new UserRemove.Handler(_inMemoryGeneralUnitOfWorkFactory,
                _mockICommander.Object);
            _users = _fakeGeneralUnitOfWork.Users;
        }

        #endregion


        [Test]
        public async Task ProcessCommand_GivenValidRequest_ShouldAddUser()
        {
            // arrange
            Setup();
            var validRequest = GetValidRequest();
            // action
            await _handler.ProcessCommand(validRequest, CancellationToken.None);
            // assert
            var user = await _users.FindOne(x => x.Id == validRequest.Id);
            user.Should().Be(null);
        }

        public UserRemove.Request GetValidRequest()
        {
            var existingUser = _fakeGeneralUnitOfWork.Users.AddAFake();
            var userDeleteUpdateModels = Builder<User>.CreateNew()
                .WithValidData()
                .With(x => x.Id = existingUser.Id)
                .Build()
                .DynamicCastTo<UserRemove.Request>();
            return userDeleteUpdateModels;
        }
    }
}