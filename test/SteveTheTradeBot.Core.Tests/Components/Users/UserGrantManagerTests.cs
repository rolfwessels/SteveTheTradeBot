using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Core.Framework.BaseManagers;
using SteveTheTradeBot.Core.Tests.Framework.BaseManagers;
using SteveTheTradeBot.Dal.Models.Users;
using SteveTheTradeBot.Dal.Persistence;
using FizzWare.NBuilder;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests.Components.Users
{
    [TestFixture]
    public class UserGrantManagerTests : BaseTypedManagerTests<UserGrant>
    {
        private Mock<ILogger<UserGrantLookup>> _mockLogger;
        private UserGrantLookup _userGrantLookup;

        #region Setup/Teardown

        public override void Setup()
        {
            base.Setup();
            _mockLogger = new Mock<ILogger<UserGrantLookup>>();
            _userGrantLookup = new UserGrantLookup(_fakeGeneralUnitOfWork.UserGrants);
        }

        #endregion

        protected override IRepository<UserGrant> Repository => _fakeGeneralUnitOfWork.UserGrants;

        protected override UserGrant SampleObject => Builder<UserGrant>.CreateNew().Build();

        protected override BaseLookup<UserGrant> Lookup => _userGrantLookup;
    }
}