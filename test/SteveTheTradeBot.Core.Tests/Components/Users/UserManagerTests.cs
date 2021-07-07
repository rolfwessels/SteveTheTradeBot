using System.Linq;
using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Core.Framework.BaseManagers;
using SteveTheTradeBot.Core.Tests.Framework.BaseManagers;
using SteveTheTradeBot.Core.Tests.Helpers;
using SteveTheTradeBot.Core.Vendor;
using SteveTheTradeBot.Dal.Models.Users;
using SteveTheTradeBot.Dal.Persistence;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;
using FluentAssertions;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests.Components.Users
{
    [TestFixture]
    public class UserManagerTests : BaseTypedManagerTests<User>
    {
        private UserLookup _userLookup;

        #region Setup/Teardown

        public override void Setup()
        {
            base.Setup();
            _userLookup = new UserLookup(_fakeGeneralUnitOfWork.Users);
        }

        #endregion


        [Test]
        public void GetUserByEmail_WhenCalledWithExistingUserWithInvalidEmail_ShouldReturnNull()
        {
            // arrange
            Setup();
            var user = _fakeGeneralUnitOfWork.Users.AddFake().First();
            // action
            var userFound = _userLookup.GetUserByEmail(user.Email + "123").Result;
            // assert
            userFound.Should().BeNull();
        }

        [Test]
        public void GetUserByEmail_WhenCalledWithExistingUserWithInvalidPassword_ShouldReturnUser()
        {
            // arrange
            Setup();
            var user = _fakeGeneralUnitOfWork.Users.AddFake().First();


            // action
            var userFound = _userLookup.GetUserByEmail(user.Email).Result;
            // assert
            userFound.Should().NotBeNull();
        }

        [Test]
        public void GetUserByEmailAndPassword_WhenCalledWithExistingUsernameAndPassword_ShouldReturnTheUser()
        {
            // arrange
            Setup();
            const string password = "sample";
            var user = _fakeGeneralUnitOfWork.Users.AddAFake(x =>
            {
                x.HashedPassword = PasswordHash.CreateHash(password);
            });
            // action
            var userFound = _userLookup.GetUserByEmailAndPassword(user.Email, password).Result;
            // assert
            userFound.Should().NotBeNull();
        }

        [Test]
        public void GetUserByEmailAndPassword_WhenCalledWithExistingUsernameWithInvalidPassword_ShouldReturnNull()
        {
            // arrange
            Setup();
            const string password = "sample";
            var user = _fakeGeneralUnitOfWork.Users.AddAFake(x =>
            {
                x.HashedPassword = PasswordHash.CreateHash(password);
            });
            // action
            var userFound = _userLookup.GetUserByEmailAndPassword(user.Email, password + 123).Result;
            // assert
            userFound.Should().BeNull();
        }

        [Test]
        public void GetUserByEmailAndPassword_WhenCalledWithInvalidUser_ShouldReturnNull()
        {
            // arrange
            Setup();
            var user = _fakeGeneralUnitOfWork.Users.AddFake().First();
            const string password = "sample";
            user.HashedPassword = PasswordHash.CreateHash(password);
            // action
            var userFound = _userLookup.GetUserByEmailAndPassword(user.Email + "123", password).Result;
            // assert
            userFound.Should().BeNull();
        }


        protected override IRepository<User> Repository => _fakeGeneralUnitOfWork.Users;

        protected override User SampleObject
        {
            get { return Builder<User>.CreateNew().With(x => x.Email = GetRandom.Email()).Build(); }
        }

        protected override BaseLookup<User> Lookup => _userLookup;
    }
}