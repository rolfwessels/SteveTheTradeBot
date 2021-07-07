using System.Linq;
using SteveTheTradeBot.Dal.Models.Users;
using Bumbershoot.Utilities.Helpers;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace SteveTheTradeBot.Dal.Tests.Validation
{
    [TestFixture]
    public class UserValidatorTests
    {
        private UserValidator _validator;

        #region Setup/Teardown

        public void Setup()
        {
            _validator = new UserValidator();
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion


        [Test]
        public void Email_GiveEmptyEmail_ShouldFail()
        {
            // arrange
            Setup();
            // assert
            _validator.ShouldHaveValidationErrorFor(user => user.Email, null as string);
        }

        [Test]
        public void Email_GiveInvalidEmail_ShouldFail()
        {
            // arrange
            Setup();
            // assert
            _validator.ShouldHaveValidationErrorFor(user => user.Email, "test");
        }

        [Test]
        public void Email_GiveValidEmail_ShouldNotFail()
        {
            // arrange
            Setup();
            // assert
            _validator.ShouldNotHaveValidationErrorFor(user => user.Email, "test@test.com");
        }

        [Test]
        public void HashedPassword_GivenEmptyPassword_ShouldFail()
        {
            // arrange
            Setup();
            // assert
            _validator.ShouldHaveValidationErrorFor(user => user.HashedPassword, "");
        }


        [Test]
        public void Name_GiveNullName_ShouldFail()
        {
            // arrange
            Setup();
            // assert
            _validator.ShouldHaveValidationErrorFor(user => user.Name, null as string);
        }


        [Test]
        public void Validate_GiveInvalidRole_ShouldFail()
        {
            // arrange
            Setup();
            var user = Builder<User>.CreateNew().WithValidData().Build();
            user.Roles.Clear();
            // action
            var validationResult = _validator.Validate(user);
            // assert
            validationResult.Errors.Select(x => x.ErrorMessage).Should().Contain("'Roles' must not be empty.");
        }

        [Test]
        public void Validate_GiveValidUserData_ShouldNotFail()
        {
            // arrange
            Setup();
            var singleObjectBuilder = Builder<User>.CreateNew();
            var user = singleObjectBuilder.WithValidData().Build();

            // action
            var validationResult = _validator.Validate(user);
            // assert
            validationResult.Errors.Select(x => x.ErrorMessage).StringJoin().Should().BeEmpty();
            validationResult.IsValid.Should().BeTrue();
        }
    }
}