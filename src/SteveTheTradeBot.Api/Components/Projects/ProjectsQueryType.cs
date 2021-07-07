using System.Reflection;
using SteveTheTradeBot.Api.GraphQl;
using SteveTheTradeBot.Api.GraphQl.DynamicQuery;
using SteveTheTradeBot.Core.Components.Projects;
using SteveTheTradeBot.Dal.Models.Auth;
using SteveTheTradeBot.Dal.Models.Projects;
using HotChocolate.Types;
using Serilog;

namespace SteveTheTradeBot.Api.Components.Projects
{
    public class ProjectsQueryType : ObjectType<ProjectsQueryType.ProjectQuery>
    {
        private readonly IProjectLookup _projects;
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        public ProjectsQueryType(IProjectLookup projects)
        {
            _projects = projects;
        }


        protected override void Configure(IObjectTypeDescriptor<ProjectQuery> descriptor)
        {
            var options = new GraphQlQueryOptions<Project, ProjectPagedLookupOptions>(_projects.GetPaged);
            Name = "Projects";

            descriptor.Field("byId")
                .Description("Get project by id")
                .Type<NonNullType<ProjectType>>()
                .Argument("id", arg => arg.Type<NonNullType<StringType>>().Description("id of the project"))
                .Resolver(x => _projects.GetById(x.ArgumentValue<string>("id")))
                .RequirePermission(Activity.ReadProject);

            descriptor.Field("paged")
                .Description("all projects paged")
                .AddOptions(options)
                .Type<NonNullType<PagedListGraphType<Project, ProjectType>>>()
                .Resolver(x => options.Paged(x))
                .RequirePermission(Activity.ReadProject);
        }

        public class ProjectQuery
        {
        }
    }
}

/* scaffolding [
    {
      "FileName": "DefaultQuery.cs",
      "Indexline": "using SteveTheTradeBot.Api.Components.Projects;",
      "InsertAbove": false,
      "InsertInline": false,
      "Lines": [
        "using SteveTheTradeBot.Api.Components.Projects;"
      ]
    },
    {
      "FileName": "DefaultQuery.cs",
      "Indexline": "Field<ProjectsSpecification>",
      "InsertAbove": false,
      "InsertInline": false,
      "Lines": [
        "Field<ProjectsSpecification>(\"projects\",resolve: context => Task.BuildFromHttpContext(new object()));"
      ]
    },
    {
      "FileName": "IocApi.cs",
      "Indexline": "\/*project*\/",
      "InsertAbove": true,
      "InsertInline": false,
      "Lines": [
            "/*project*\/",
            "builder.RegisterType<ProjectType>().SingleInstance();",
            "builder.RegisterType<ProjectsSpecification>().SingleInstance();",
            "builder.RegisterType<ProjectCreateUpdateType>().SingleInstance();",
            "builder.RegisterType<ProjectsMutationType>().SingleInstance();",
            "",
      ]
    }
       

         
] scaffolding */