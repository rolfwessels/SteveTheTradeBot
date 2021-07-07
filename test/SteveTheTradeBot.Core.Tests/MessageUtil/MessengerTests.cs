using System;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using FluentAssertions;
using NUnit.Framework;

namespace SteveTheTradeBot.Core.Tests.MessageUtil
{
    [TestFixture]
    public class MessengerTests
    {
        private IMessenger _messenger;

        #region Setup/Teardown

        public void Setup()
        {
            _messenger = new Messenger();
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion

        
        [Test]
        public async Task Send_Given_Object_ShouldBeReceived()
        {
            // arrange
            Setup();
            var o = new object();
            string received = null;
            _messenger.Register<SampleMessage>(o, m => received = m.Message);
            // action
            await _messenger.Send(new SampleMessage("String"));
            // assert
            received.Should().NotBeNull();
        }

        [Test]
        public async Task Send_GivenObject_ShouldBeReceivedOnOtherListener()
        {
            // arrange
            Setup();
            var o = new object();
            object received = null;
            _messenger.Register(typeof(SampleMessage), o, m => received = m);
            // action
            await _messenger.Send(new SampleMessage("String"));
            // assert
            received.Should().NotBeNull();
        }

        [Test]
        public void Send_GivenRegisteredAndThenUnRegister_ShouldNotRelieveMessage()
        {
            // arrange
            Setup();
            var o = new object();
            string received = null;
            _messenger.Register<SampleMessage>(o, m => received = m.Message);
            _messenger.UnRegister<SampleMessage>(o);
            // action
            _messenger.Send(new SampleMessage("String"));
            // assert
            received.Should().BeNull();
        }

        [Test]
        public void Send_GivenRegisteredAndThenUnRegisterAll_ShouldNotRelieveMessage()
        {
            // arrange
            Setup();
            var o = new object();
            string received = null;
            _messenger.Register<SampleMessage>(o, m => received = m.Message);
            _messenger.UnRegister(o);
            // action
            _messenger.Send(new SampleMessage("String"));
            // assert
            received.Should().BeNull();
        }

        #region Nested type: SampleMessage

        public class SampleMessage : IDisposable
        {
            public SampleMessage(string message)
            {
                Message = message;
            }

            public string Message { get; private set; }

            #region IDisposable Members

            #region Implementation of IDisposable

            public void Dispose()
            {
                Message = null;
            }

            #endregion

            #endregion
        }

        #endregion
    }
}