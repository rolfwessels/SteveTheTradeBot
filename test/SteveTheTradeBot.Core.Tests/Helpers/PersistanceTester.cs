using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SteveTheTradeBot.Dal.Models.Base;
using SteveTheTradeBot.Dal.Persistence;
using FluentAssertions;

namespace SteveTheTradeBot.Core.Tests.Helpers
{
    public class PersistanceTester<T> where T : IBaseDalModelWithId
    {
        private readonly Func<IGeneralUnitOfWork, IRepository<T>> _repo;
        private readonly List<Action<T, T>> _testSaved = new List<Action<T, T>>();
        private readonly List<Action<T, T>> _testUpdated = new List<Action<T, T>>();
        private readonly IGeneralUnitOfWork _unitOfWork;

        public PersistanceTester(IGeneralUnitOfWork unitOfWork, Func<IGeneralUnitOfWork, IRepository<T>> repo)
        {
            _unitOfWork = unitOfWork;
            _repo = repo;
        }

        public async Task ValidateCrud(T user)
        {
            var repository = _repo(_unitOfWork);

            var findFirst = await repository.FindOne(x => x.Id == user.Id);
            findFirst.Should().BeNull("Could not load the value");
            var add = await repository.Add(user);
            foreach (var action in _testSaved)
            {
                var firstOrDefault = await repository.FindOne(x => x.Id == user.Id);
                firstOrDefault.Should().NotBeNull("Could not load the value");
                action(user, firstOrDefault);
            }

            add.Should().NotBeNull("Saving should return the saved value");
            var remove = await repository.Remove(x => x.Id == add.Id);
            remove.Should().BeTrue("Remove record should return true");


            var afterDelete = await repository.FindOne(x => x.Id == user.Id);
            afterDelete.Should().BeNull("Item removed but could still be found");

            var removeTest = await repository.Remove(x => x.Id == add.Id);
            removeTest.Should().BeFalse("Remove record not be removed");
        }

        public void ValueValidate<TType>(Expression<Func<T, TType>> func, TType value, TType value2)
        {
            var compile = func.Compile();
            _testSaved.Add((type, newValue) => compile(type).Should()
                .Be(compile(type), $"Original value for {func} not saved"));
            _testUpdated.Add((type, newValue) => compile(type).Should()
                .Be(compile(type), $"Original value for {func} not saved"));
        }
    }
}