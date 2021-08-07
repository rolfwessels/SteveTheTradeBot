using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using SlackConnector;
using SlackConnector.Models;
using SteveTheTradeBot.Core.Utils;

namespace SteveTheTradeBot.Core.Framework.Slack
{
    public class SlackService : IDisposable
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _key;
        private readonly ISlackConnector _connector;
        private ISlackConnection _connection;
        private readonly List<IResponder> _responders;
        private readonly TimeSpan _reconnectTime = TimeSpan.FromMinutes(5);
        public int ReconnectingCounter { get; set; }

        public SlackService(string key, ResponseBuilder responseBuilder)
        {
            _key = key;
            _connector = new SlackConnector.SlackConnector();
            _responders = responseBuilder.GetResponders();
        }

        public async Task Connect()
        {
            _log.Information("Connecting to slack service");
            _connection = await _connector.Connect(_key);
            _log.Information("Connected");
            LinkEvents();
        }

        private void LinkEvents(bool linkEvents = true)
        {
            if (_connection == null) return;
            _connection.OnMessageReceived -= MessageReceived;
            _connection.OnDisconnect -= OnDisconnectedTryReconnect;
            _connection.OnReconnecting -= Reconnecting;
            if (linkEvents)
            {
                _connection.OnMessageReceived += MessageReceived;
                _connection.OnDisconnect += OnDisconnectedTryReconnect;
                _connection.OnReconnecting += Reconnecting;
            }
        }


        private async Task Reconnecting()
        {
            ReconnectingCounter++;
            _log.Debug($"OnReconnecting {ReconnectingCounter}");
            if (ReconnectingCounter > 30)
            {
                Close();
                _log.Debug($"Wait a few {_reconnectTime.ToShort()} then reconnect...");
                await Task.Delay(_reconnectTime);
                _log.Debug($"Trying to reconnect!");
                await Connect();
            }
        }

        private void OnDisconnectedTryReconnect()
        {
            _log.Information("Disconnected. Slack will try to reconnect on its own.");
        }

        private void Close()
        {
            if (_connection == null) return;
            LinkEvents(false);
            try
            {
                _connection.Close().Wait();
                _connection = null;
            }
            catch (Exception e)
            {
                _log.Warning("SlackService Ensure disconnected: " + e.Message);
            }
        }

        private async Task MessageReceived(SlackMessage message)
        {
            
            var messageContext = GetMessageContext(message);
            try
            {
                await ProcessMessage(messageContext);
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
                _connection.Say(new BotMessage()
                {
                    Text = $"Ooops something went wrong ({e.Message})",
                    ChatHub = message.ChatHub
                }).Wait();
            }
        }

        private MessageContext GetMessageContext(SlackMessage message)
        {
            const bool botHasResponded = false;
            var messageContext = new MessageContext(message, botHasResponded, _connection);

            return messageContext;
        }

        private async Task ProcessMessage(MessageContext messageContext)
        {
            _log.Debug($"Message in {messageContext.Message.User.Name}: {messageContext.Message.Text}");
            foreach (var responder in _responders)
            {
                if (responder.CanRespond(messageContext))
                {
                    var botMessage = responder.GetResponse(messageContext);
                    if (botMessage != null)
                    {
                        if (botMessage.ChatHub == null)
                        {
                            botMessage.ChatHub = messageContext.Message.ChatHub;
                        }
                        await _connection.Say(botMessage);
                        _log.Debug($"Message out {messageContext.Message.User.Name}: {botMessage.Text}");
                        messageContext.BotHasResponded = true;
                    }
                }
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}