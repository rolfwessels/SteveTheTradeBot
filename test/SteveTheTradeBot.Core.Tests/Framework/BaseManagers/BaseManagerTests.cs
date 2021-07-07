using System;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Dal.InMemoryCollections;
using SteveTheTradeBot.Dal.Persistence;
using FluentAssertions.Equivalency;
using FluentValidation;
using Moq;
using NUnit.Framework;
using IValidatorFactory = SteveTheTradeBot.Dal.Validation.IValidatorFactory;
using ValidatorFactoryBase = SteveTheTradeBot.Dal.Validation.ValidatorFactoryBase;

namespace SteveTheTradeBot.Core.Tests.Framework.BaseManagers
{
    [TestFixture]
    public class BaseManagerTests
    {
        
        protected IGeneralUnitOfWork _fakeGeneralUnitOfWork;
        public InMemoryGeneralUnitOfWorkFactory _inMemoryGeneralUnitOfWorkFactory;
        public Mock<ICommander> _mockICommander;
        protected Mock<IMessenger> _mockIMessenger;
        protected Mock<IValidatorFactory> _mockIValidatorFactory;

        #region Setup/Teardown

        public virtual void Setup()
        {
            _mockIMessenger = new Mock<IMessenger>();
            _mockIValidatorFactory = new Mock<IValidatorFactory>();
            _inMemoryGeneralUnitOfWorkFactory = new InMemoryGeneralUnitOfWorkFactory();
            _fakeGeneralUnitOfWork = _inMemoryGeneralUnitOfWorkFactory.GetConnection();
            _mockICommander = new Mock<ICommander>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _mockIValidatorFactory.VerifyAll();
            _mockIMessenger.VerifyAll();
            _mockICommander.VerifyAll();
        }

        #endregion

        protected static EquivalencyAssertionOptions<T> DefaultCommandExcluding<T>(
            EquivalencyAssertionOptions<T> opt) where T : CommandRequestBase
        {
            return opt
                .Excluding(x => x.CreatedAt)
                .Excluding(x => x.CorrelationId);
        }

        #region Nested type: FakeValidator

        internal class FakeValidator : ValidatorFactoryBase
        {
            private readonly object _createInstance;

            public FakeValidator(object createInstance)
            {
                _createInstance = createInstance;
            }

            #region Overrides of ValidatorFactoryBase

            protected override void TryResolve<T>(out IValidator<T> output)
            {
                output = (IValidator<T>) _createInstance;
            }

            #endregion

            public static IValidatorFactory New<T>()
            {
                return new FakeValidator(Activator.CreateInstance(typeof(T)));
            }
        }

        #endregion
    }
}