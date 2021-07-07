using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteveTheTradeBot.Dal.Models.Projects;
using SteveTheTradeBot.Sdk.RestApi.Clients;
using SteveTheTradeBot.Shared.Models.Projects;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Dal.Tests;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;

namespace SteveTheTradeBot.Api.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class ProjectApiClientTests : IntegrationTestsBase
    {
        private ProjectApiClient _projectApiClient;

        #region Setup/Teardown

        protected void Setup()
        {
            _projectApiClient = _adminConnection.Value.Projects;
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion

        [Test]
        public async Task ProjectCrud_GivenInsertUpdateDelete_ShouldBeValid()
        {
            // arrange
            Setup();
            var data = GetExampleData();
            var projectCreate = data.First();
            var projectUpdate = data.Last();
            // action
            await _projectApiClient.All();

            var createCommand = await _projectApiClient.Create(projectCreate);
            var create = await _projectApiClient.ById(createCommand.Id);
            var updateCommand = await _projectApiClient.Update(create.Id, projectUpdate);
            var update = await _projectApiClient.ById(create.Id);
            var allAfterUpdate = await _projectApiClient.All();
            var paged = await _projectApiClient.Paged(1);
            var firstDelete = await _projectApiClient.Remove(create.Id);

            // assert
            create.Should().BeEquivalentTo(projectCreate, CompareConfig);
            update.Should().BeEquivalentTo(projectUpdate, CompareConfig);
            allAfterUpdate.Count.Should().BeGreaterThan(0);
            allAfterUpdate.Should().Contain(x => x.Name == update.Name);
            paged.Count.Should().BeGreaterThan(paged.Items.Count);
            paged.Items.Should().HaveCountLessOrEqualTo(1);
            firstDelete.Id.Should().Be(createCommand.Id);
        }

        [Test]
        public void Create_GivenInvalidModel_ShouldFail()
        {
            // arrange
            Setup();
            var invalidEmailProject = GetExampleData().First();
            invalidEmailProject.Name = "";
            // action
            Action testUpdateValidationFail = () => { _projectApiClient.Create(invalidEmailProject).Wait(); };
            // assert
            testUpdateValidationFail.Should().Throw<Exception>()
                .WithMessage("'Name' must be between 1 and 150 characters. You entered 0 characters.");
        }

        [Test]
        public void Create_GivenGuestProject_ShouldFail()
        {
            // arrange
            Setup();
            var invalidEmailProject = GetExampleData().First();
            // action
            Action testUpdateValidationFail = () =>
            {
                _guestConnection.Value.Projects.Create(invalidEmailProject).Wait();
            };
            // action
            testUpdateValidationFail.Should().Throw<Exception>()
                .WithMessage("The current user is not authorized to access this resource.");
        }

        #region Overrides of CrudComponentTestsBase<ProjectModel,ProjectCreateUpdateModel,ProjectReferenceModel>

        protected EquivalencyAssertionOptions<ProjectCreateUpdateModel> CompareConfig(
            EquivalencyAssertionOptions<ProjectCreateUpdateModel> options)
        {
            return options;
        }

        #endregion

        #region Overrides of CrudComponentTestsBase<ProjectModel,ProjectCreateUpdateModel>

        protected IList<ProjectCreateUpdateModel> GetExampleData()
        {
            var projectCreateUpdateModels = Builder<Project>.CreateListOfSize(2).WithValidData().Build()
                .DynamicCastTo<List<ProjectCreateUpdateModel>>();
            return projectCreateUpdateModels;
        }

        #endregion
    }
}