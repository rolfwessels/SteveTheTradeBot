using SteveTheTradeBot.Dal.Models.Projects;
using SteveTheTradeBot.Dal.Models.SystemEvents;
using SteveTheTradeBot.Dal.Models.Users;
using SteveTheTradeBot.Dal.Persistence;
using MongoDB.Driver;

namespace SteveTheTradeBot.Dal.MongoDb
{
    public class MongoGeneralUnitOfWork : IGeneralUnitOfWork
    {
        public MongoGeneralUnitOfWork(IMongoDatabase database)
        {
            Users = new MongoRepository<User>(database);
            Projects = new MongoRepository<Project>(database);
            UserGrants = new MongoRepository<UserGrant>(database);
            SystemCommands = new MongoRepository<SystemCommand>(database);
            SystemEvents = new MongoRepository<SystemEvent>(database);
        }

        #region IGeneralUnitOfWork Members

        #region Implementation of IDisposable

        public void Dispose()
        {
        }

        #endregion

        #endregion

        #region Implementation of IGeneralUnitOfWork

        public IRepository<User> Users { get; }
        public IRepository<Project> Projects { get; }
        public IRepository<UserGrant> UserGrants { get; }
        public IRepository<SystemCommand> SystemCommands { get; set; }
        public IRepository<SystemEvent> SystemEvents { get; set; }

        #endregion
    }
}