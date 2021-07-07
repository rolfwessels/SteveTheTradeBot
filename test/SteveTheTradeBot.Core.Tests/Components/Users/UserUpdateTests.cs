using System;
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
using FluentValidation;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests.Components.Users
{
    [TestFixture]
    public class UserUpdateTests : BaseManagerTests
    {
        private UserUpdate.Handler _handler;
        private IRepository<User> _users;

        #region Setup/Teardown

        public override void Setup()
        {
            base.Setup();
            _handler = new UserUpdate.Handler(_inMemoryGeneralUnitOfWorkFactory, FakeValidator.New<UserValidator>(),
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
            validRequest.Name = "";
            // action
            Action testCall = () => { _handler.ProcessCommand(validRequest, CancellationToken.None).Wait(); };
            // assert
            testCall.Should().Throw<ValidationException>()
                .And.Errors.Should().Contain(x =>
                    x.ErrorMessage == "'Name' must be between 1 and 150 characters. You entered 0 characters.");
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
            ;
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
            user.Should().BeEquivalentTo(validRequest, opt => DefaultCommandExcluding(opt)
                .Excluding(x => x.Password));
            user.HashedPassword.Length.Should().BeGreaterThan(validRequest.Password.Length);
        }

        [Test]
        public async Task ProcessCommand_GivenValidRequest_ShouldSetPassword()
        {
            // arrange
            Setup();
            var validRequest = GetValidRequest();
            validRequest.Password = "test";
            // action
            await _handler.ProcessCommand(validRequest, CancellationToken.None);
            // assert
            var user = await _users.FindOne(x => x.Id == validRequest.Id);
            user.IsPassword("test").Should().Be(true);
        }

        [Test]
        public async Task ProcessCommand_GivenPasswordAsNull_ShouldNotSetPassword()
        {
            // arrange
            Setup();
            var validRequest = GetValidRequest();
            validRequest.Password = null;
            // action
            await _handler.ProcessCommand(validRequest, CancellationToken.None);
            // assert
            var user = await _users.FindOne(x => x.Id == validRequest.Id);
            user.IsPassword("existingpass").Should().Be(true);
        }

        public UserUpdate.Request GetValidRequest()
        {
            var existingUser = _fakeGeneralUnitOfWork.Users.AddAFake(x => UserDalHelper.SetPassword(x, "existingpass"));
            var userUpdateUpdateModels = Builder<User>.CreateNew()
                .WithValidData()
                .With(x => x.Id = existingUser.Id)
                .Build()
                .DynamicCastTo<UserUpdate.Request>();
            userUpdateUpdateModels.Password = "tes";
            return userUpdateUpdateModels;
        }
    }
}