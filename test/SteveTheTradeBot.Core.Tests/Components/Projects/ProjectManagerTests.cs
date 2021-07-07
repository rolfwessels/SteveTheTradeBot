using SteveTheTradeBot.Core.Components.Projects;
using SteveTheTradeBot.Core.Framework.BaseManagers;
using SteveTheTradeBot.Core.Tests.Framework.BaseManagers;
using SteveTheTradeBot.Dal.Models.Projects;
using SteveTheTradeBot.Dal.Persistence;
using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests.Components.Projects
{
    [TestFixture]
    public class ProjectManagerTests : BaseTypedManagerTests<Project>
    {
        private Mock<ILogger<ProjectLookup>> _mockLogger;
        private ProjectLookup _projectLookup;

        #region Setup/Teardown

        public override void Setup()
        {
            base.Setup();
            _mockLogger = new Mock<ILogger<ProjectLookup>>();
            _projectLookup = new ProjectLookup(_fakeGeneralUnitOfWork.Projects);
        }

        #endregion

        protected override IRepository<Project> Repository => _fakeGeneralUnitOfWork.Projects;

        protected override Project SampleObject => Builder<Project>.CreateNew().Build();

        protected override BaseLookup<Project> Lookup => _projectLookup;
    }
}