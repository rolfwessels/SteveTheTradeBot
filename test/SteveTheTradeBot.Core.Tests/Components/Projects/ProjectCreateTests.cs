using System;
using System.Threading;
using System.Threading.Tasks;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Core.Components.Projects;
using SteveTheTradeBot.Core.Tests.Framework.BaseManagers;
using SteveTheTradeBot.Dal.Models.Projects;
using SteveTheTradeBot.Dal.Persistence;
using SteveTheTradeBot.Dal.Tests;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests.Components.Projects
{
    [TestFixture]
    public class ProjectCreateTests : BaseManagerTests
    {
        private ProjectCreate.Handler _handler;
        private IRepository<Project> _projects;

        #region Setup/Teardown

        public override void Setup()
        {
            base.Setup();
            _handler = new ProjectCreate.Handler(_inMemoryGeneralUnitOfWorkFactory,
                FakeValidator.New<ProjectValidator>(),
                _mockICommander.Object);
            _projects = _fakeGeneralUnitOfWork.Projects;
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
        public async Task ProcessCommand_GivenValidRequest_ShouldAddProject()
        {
            // arrange
            Setup();
            var validRequest = GetValidRequest();
            // action
            await _handler.ProcessCommand(validRequest, CancellationToken.None);
            // assert
            var project = await _projects.FindOne(x => x.Id == validRequest.Id);
            project.Should().NotBeNull();
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
            var project = await _projects.FindOne(x => x.Id == validRequest.Id);
            project.Should().BeEquivalentTo(validRequest, DefaultCommandExcluding);
        }

        public ProjectCreate.Request GetValidRequest()
        {
            var projectCreateUpdateModels = Builder<Project>.CreateNew().WithValidData().Build()
                .DynamicCastTo<ProjectCreate.Request>();
            return projectCreateUpdateModels;
        }
    }
}