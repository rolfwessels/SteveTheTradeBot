using SteveTheTradeBot.Shared.Interfaces.Base;
using SteveTheTradeBot.Shared.Models.Projects;

namespace SteveTheTradeBot.Shared.Interfaces.Shared
{
    public interface IProjectControllerActions : ICrudController<ProjectModel, ProjectCreateUpdateModel>
    {
    }
}