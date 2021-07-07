using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using MongoDB.Driver;

namespace SteveTheTradeBot.Dal.MongoDb.Migrations
{
    public class VersionUpdater
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object _locker = new object();
        private readonly IMigration[] _updates;

        public VersionUpdater(IMigration[] updates)
        {
            _updates = updates;
        }

        public Task Update(IMongoDatabase db)
        {
            return Task.Run(() =>
            {
                lock (_locker)
                {
                    var repository = new MongoRepository<DbVersion>(db);
                    var versions = repository.Find().Result;
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    for (var i = 0; i < _updates.Length; i++)
                    {
                        var migrateInitialize = _updates[i];
                        EnsureThatVersionDoesNotExistThenUpdate(versions, i, migrateInitialize, repository, db).Wait();
                    }

                    stopwatch.Stop();
                    _log.Information(
                        $"Found {versions.Count} database updates in database and {_updates.Length} in code. Update took [{stopwatch.ElapsedMilliseconds}]");
                }
            });
        }

        #region Private Methods

        private async Task EnsureThatVersionDoesNotExistThenUpdate(IEnumerable<DbVersion> versions, int i,
            IMigration migrateInitialize, MongoRepository<DbVersion> repository, IMongoDatabase db)
        {
            var version = versions.FirstOrDefault(x => x.Id == i);
            if (version == null)
            {
                _log.Information($"Running version update {migrateInitialize.GetType().Name}");
                await RunTheUpdate(migrateInitialize, db);
                var dbVersion1 = new DbVersion {Id = i, Name = migrateInitialize.GetType().Name};
                await repository.Add(dbVersion1);
            }
        }

        private async Task RunTheUpdate(IMigration migrateInitialize, IMongoDatabase db)
        {
            _log.Information($"Starting {migrateInitialize.GetType().Name} db update");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await migrateInitialize.Update(db);
            stopwatch.Stop();
            _log.Information($"Done {migrateInitialize.GetType().Name} in {stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion
    }
}