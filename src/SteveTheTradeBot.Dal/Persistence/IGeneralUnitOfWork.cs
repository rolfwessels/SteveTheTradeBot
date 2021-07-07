using System;
using SteveTheTradeBot.Dal.Models.Projects;
using SteveTheTradeBot.Dal.Models.SystemEvents;
using SteveTheTradeBot.Dal.Models.Users;

namespace SteveTheTradeBot.Dal.Persistence
{
    public interface IGeneralUnitOfWork : IDisposable
    {
        IRepository<Project> Projects { get; }
        IRepository<User> Users { get; }
        IRepository<UserGrant> UserGrants { get; }
        IRepository<SystemCommand> SystemCommands { get; set; }
        IRepository<SystemEvent> SystemEvents { get; set; }
    }
}