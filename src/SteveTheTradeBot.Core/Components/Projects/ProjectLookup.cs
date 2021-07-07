using System;
using System.Linq;
using System.Threading.Tasks;
using SteveTheTradeBot.Core.Framework.BaseManagers;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Dal.Models.Projects;
using SteveTheTradeBot.Dal.Persistence;

namespace SteveTheTradeBot.Core.Components.Projects
{
    public class ProjectLookup : BaseLookup<Project>, IProjectLookup
    {
        public ProjectLookup(IRepository<Project> projects)
        {
            Repository = projects;
        }

        #region Overrides of BaseLookup<Project>

        protected override IRepository<Project> Repository { get; }

        #endregion

        #region Implementation of IProjectLookup

        public Task<PagedList<Project>> GetPaged(ProjectPagedLookupOptions options)
        {
            return Task.Run(() =>
            {
                var query = Repository.Query();
                if (!string.IsNullOrEmpty(options.Search))
                    query = query.Where(x =>
                        x.Id.ToLower().Contains(options.Search.ToLower()) ||
                        x.Name.ToLower().Contains(options.Search.ToLower()));

                if (options.Sort != null)
                    switch (options.Sort)
                    {
                        case ProjectPagedLookupOptions.SortOptions.Name:
                            query = query.OrderBy(x => x.Name);
                            break;
                        case ProjectPagedLookupOptions.SortOptions.Recent:
                            query = query.OrderByDescending(x => x.UpdateDate);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                return new PagedList<Project>(query, options);
            });
        }

        #endregion
    }
}

/* scaffolding [
    {
      "FileName": "IocCoreBase.cs",
      "Indexline": "As<IUserLookup>",
      "InsertAbove": false,
      "InsertInline": false,
      "Lines": [
        "builder.RegisterType<ProjectLookup>().As<IProjectLookup>();"
      ]
    },
    {
      "FileName": "MongoGeneralUnitOfWork.cs",
      "Indexline": "Projects = ",
      "InsertAbove": false,
      "InsertInline": false,
      "Lines": [
        "Projects = new MongoRepository<Project>(database);"
      ]
    },
    {
      "FileName": "IGeneralUnitOfWork.cs",
      "Indexline": "Projects",
      "InsertAbove": false,
      "InsertInline": false,
      "Lines": [
        "IRepository<Project> Projects { get; }"
      ]
    },
    {
      "FileName": "MongoGeneralUnitOfWork.cs",
      "Indexline": "Projects { get; private set; }",
      "InsertAbove": false,
      "InsertInline": false,
      "Lines": [
        "public IRepository<Project> Projects { get; private set; }"
      ]
    },
  {
      "FileName": "InMemoryGeneralUnitOfWork.cs",
      "Indexline": "Projects =",
      "InsertAbove": false,
      "InsertInline": false,
      "Lines": [
        "Projects = new FakeRepository<Project>();"
      ]
    },
    {
      "FileName": "InMemoryGeneralUnitOfWork.cs",
      "Indexline": "Projects { get",
      "InsertAbove": false,
      "InsertInline": false,
      "Lines": [
        "public IRepository<Project> Projects { get; private set; }"
      ]
    }
] scaffolding */