using System;
using System.Linq;
using SteveTheTradeBot.Dal.Models.Base;
using Bumbershoot.Utilities.Helpers;

namespace SteveTheTradeBot.Dal.Models.SystemEvents
{
    public class SystemEvent : BaseDalModelWithId
    {
        public SystemEvent(string correlationId, DateTime createdAt, string eventId, string eventName, string typeName, string data)
        {
            CorrelationId = correlationId;
            CreatedAt = createdAt;
            EventName = eventName;
            EventId = eventId;
            TypeName = typeName;
            Data = data;
        }

        public string CorrelationId { get;  }
        public DateTime CreatedAt { get; set; }
        public string EventName { get; set; }
        public string EventId { get; set; }
        public string TypeName { get; set; }
        public string Data { get; set; }


        public static string BuildTypeName<T>(T commandRequest)
        {
            var type = commandRequest?.GetType() ?? typeof(T);
            return type.FullName.OrEmpty().Split(".").Last();
        }
    }
}