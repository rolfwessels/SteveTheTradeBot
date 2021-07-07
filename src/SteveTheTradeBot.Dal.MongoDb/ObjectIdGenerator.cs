using SteveTheTradeBot.Dal.Persistence;
using MongoDB.Bson;

namespace SteveTheTradeBot.Dal.MongoDb
{
    public class ObjectIdGenerator : IIdGenerator
    {
        #region Implementation of IIdGenerator

        public string NewId => ObjectId.GenerateNewId().ToString();

        #endregion
    }
}