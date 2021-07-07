using System.Threading.Tasks;
using SteveTheTradeBot.Dal.MongoDb.Migrations;
using SteveTheTradeBot.Dal.MongoDb.Migrations.Versions;
using MongoDB.Driver;

namespace SteveTheTradeBot.Dal.MongoDb
{
    public class Configuration
    {
        private static readonly object _locker = new object();
        private static Configuration _instance;
        private readonly IMigration[] _updates;
        private MongoMappers _mongoMappers;
        private Task _update;

        protected Configuration()
        {
            _updates = new IMigration[]
            {
                new MigrateInitialize()
            };
        }

        public Task Update(IMongoDatabase db)
        {
            lock (_instance)
            {
                if (_update == null)
                {
                    _mongoMappers = new MongoMappers();
                    _mongoMappers.InitializeMappers();
                    var versionUpdater = new VersionUpdater(_updates);
                    _update = versionUpdater.Update(db);
                }
            }

            return _update;
        }

        #region Instance

        public static Configuration Instance()
        {
            if (_instance == null)
                lock (_locker)
                {
                    if (_instance == null) _instance = new Configuration();
                }

            return _instance;
        }

        #endregion
    }
}