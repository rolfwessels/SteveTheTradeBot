using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteveTheTradeBot.Dal.Models.Users;
using SteveTheTradeBot.Sdk.RestApi;
using SteveTheTradeBot.Sdk.RestApi.Clients;
using SteveTheTradeBot.Shared.Models.Users;
using Bumbershoot.Utilities.Helpers;
using SteveTheTradeBot.Dal.Tests;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;
using FluentAssertions;
using NUnit.Framework;

namespace SteveTheTradeBot.Api.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class SubscriptionsClientTests : IntegrationTestsBase
    {
        private UserApiClient _userApiClient;

        #region Setup/Teardown

        protected void Setup()
        {
            TestLoggingHelper.EnsureExists();
            _userApiClient = _adminConnection.Value.Users;
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion

        [Test]
        public async Task OnDefaultEvent_GivenInsertUpdateDelete_ShouldBeValid()
        {
            // arrange
            Setup();
            var userCreate = GetExampleData().First();
            var items = new List<SteveTheTradeBotClient.RealTimeEvent>();
            var sendSubscribeGeneralEvents = _adminConnection.Value.SendSubscribeGeneralEvents();
            Exception error = null;
            void OnError(Exception e) => error = e;
            var subscriptions =
                sendSubscribeGeneralEvents.Subscribe(evt => items.Add(evt.Data.OnDefaultEvent), OnError);

            using (subscriptions)
            {
                await Task.Delay(100);//required to allow subscription
                // action
                var insertCommand = await _userApiClient.Create(userCreate);
                var insert = await _userApiClient.ById(insertCommand.Id);
                await _userApiClient.Remove(insert.Id);

                items.WaitFor(x => x.Count >= 2, 10000);
                items.Select(x => x.Event).Should().Contain("UserRemoved");
                items.Should().HaveCountGreaterOrEqualTo(2);
                error.Should().BeNull();
                
            }

            subscriptions.Should().NotBeNull();
        }


        #region Overrides of CrudComponentTestsBase<UserModel,UserCreateUpdateModel>

        protected IList<UserCreateUpdateModel> GetExampleData()
        {
            var userCreateUpdateModels = Builder<User>.CreateListOfSize(2).WithValidData().Build()
                .DynamicCastTo<List<UserCreateUpdateModel>>();
            userCreateUpdateModels.ForEach(x => x.Password = GetRandom.Phrase(20));
            return userCreateUpdateModels;
        }

        #endregion
    }
}