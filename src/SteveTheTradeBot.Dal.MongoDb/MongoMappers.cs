using System;
using System.Reflection;
using SteveTheTradeBot.Dal.Models.Base;
using Serilog;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace SteveTheTradeBot.Dal.MongoDb
{
    public class MongoMappers
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public void InitializeMappers()
        {
            SetupDataTimeSerializer();
            SetupMapping();
        }

        #region Private Methods

        private static void SetupMapping()
        {
            BsonClassMap.RegisterClassMap<BaseDalModel>(cm =>
            {
                cm.MapProperty(c => c.CreateDate).SetElementName("Cd");
                cm.MapProperty(c => c.UpdateDate).SetElementName("Ud");
            });
            BsonClassMap.RegisterClassMap<BaseDalModelWithId>(cm =>
            {
                cm.MapIdProperty(c => c.Id)
                    .SetIdGenerator(StringObjectIdGenerator.Instance)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        private static void SetupDataTimeSerializer()
        {
            try
            {
                var serializer = new DateTimeSerializer(DateTimeKind.Local);
                BsonSerializer.RegisterSerializer(typeof(DateTime), serializer);
            }
            catch (Exception e)
            {
                _log.Error("MongoMappers:InitializeMappers " + e.Message, e);
            }
        }

        #endregion
    }
}