using System;
using System.Reflection;
using SteveTheTradeBot.Dal.Persistence;
using Serilog;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SteveTheTradeBot.Dal.MongoDb
{
    public class MongoConnectionFactory : IGeneralUnitOfWorkFactory
    {
        private readonly Lazy<IGeneralUnitOfWork> _singleConnection;

        public MongoConnectionFactory(string connectionString, string databaseName)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
            _singleConnection = new Lazy<IGeneralUnitOfWork>(GeneralUnitOfWork);
        }

        public string DatabaseName { get; }

        public string ConnectionString { get; }

        #region IGeneralUnitOfWorkFactory Members

        public IGeneralUnitOfWork GetConnection()
        {
            return _singleConnection.Value;
        }

        public string NewId => ObjectId.GenerateNewId().ToString();

        #endregion

        public IMongoDatabase DatabaseOnly()
        {
            var client = ClientOnly();
            var database = client.GetDatabase(DatabaseName);
            return database;
        }

        #region Private Methods

        private IGeneralUnitOfWork GeneralUnitOfWork()
        {
            var database = DatabaseOnly();
            Configuration.Instance().Update(database).Wait();
            return new MongoGeneralUnitOfWork(database);
        }

        private IMongoClient ClientOnly()
        {
            return new MongoClient(ConnectionString);
        }

        #endregion
    }
}