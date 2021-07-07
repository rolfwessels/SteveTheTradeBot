using System;
using System.Threading;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Core.Tests.Framework.BaseManagers;
using SteveTheTradeBot.Dal.Models.Users;
using SteveTheTradeBot.Dal.Persistence;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Dal.Tests;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests.Components.Users
{
    [TestFixture]
    public class UserCreateTests : BaseManagerTests
    {
        private UserCreate.Handler _handler;
        private IRepository<User> _users;

        #region Setup/Teardown

        public override void Setup()
        {
            base.Setup();
            _handler = new UserCreate.Handler(_inMemoryGeneralUnitOfWorkFactory, FakeValidator.New<UserValidator>(),
                _mockICommander.Object);
            _users = _fakeGeneralUnitOfWork.Users;
        }

        #endregion

        [Test]
        public void ProcessCommand_GivenInvalidRequest_ShouldThrowException()
        {
            // arrange
            Setup();
            var validRequest = GetValidRequest();
            validRequest.Email = "sere";
            // action
            Action testCall = () => { _handler.ProcessCommand(validRequest, CancellationToken.None).Wait(); };
            // assert
            testCall.Should().Throw<ValidationException>()
                .And.Errors.Should().Contain(x => x.ErrorMessage == "'Email' is not a valid email address.");
        }

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
            user.Should().NotBeNull();
        }

        [Test]
        public async Task ProcessCommand_GivenValidRequest_ShouldSetAllProperties()
        {
            // arrange
            Setup();
            var validRequest = GetValidRequest();
            // action
            await _handler.ProcessCommand(validRequest, CancellationToken.None);
            // assert
            var user = await _users.FindOne(x => x.Id == validRequest.Id);
            user.Should().BeEquivalentTo(validRequest, opt => DefaultCommandExcluding(opt).Excluding(x => x.Password));
            user.HashedPassword.Length.Should().BeGreaterThan(validRequest.Password.Length);
        }

        public UserCreate.Request GetValidRequest()
        {
            var userCreateUpdateModels = Builder<User>.CreateNew().WithValidData().Build()
                .DynamicCastTo<UserCreate.Request>();
            userCreateUpdateModels.Password = "password";
            return userCreateUpdateModels;
        }
    }
}