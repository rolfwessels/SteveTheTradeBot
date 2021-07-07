using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.BaseManagers;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Dal.Models.Projects;

namespace SteveTheTradeBot.Core.Components.Projects
{
    public interface IProjectLookup : IBaseLookup<Project>
    {
        Task<PagedList<Project>> GetPaged(ProjectPagedLookupOptions options);
    }
}