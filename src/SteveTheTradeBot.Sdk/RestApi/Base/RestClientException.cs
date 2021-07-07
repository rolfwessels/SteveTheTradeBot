using System;
using SteveTheTradeBot.Shared.Models.Shared;

namespace SteveTheTradeBot.Sdk.RestApi.Base
{
    public class RestClientException : Exception
    {
        public RestClientException(ErrorMessage message, Exception innerException) : base(message.Message,
            innerException)
        {
            FullMessage = message;
        }

        public RestClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ErrorMessage FullMessage { get; set; }
    }
}