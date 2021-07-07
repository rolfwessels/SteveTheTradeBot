using SteveTheTradeBot.Api.GraphQl;
using SteveTheTradeBot.Dal.Models.Auth;
using SteveTheTradeBot.Shared.Models.Projects;
using HotChocolate.Types;

namespace SteveTheTradeBot.Api.Components.Projects
{
    public class ProjectsMutationType : ObjectType<ProjectsMutation>
    {
        protected override void Configure(IObjectTypeDescriptor<ProjectsMutation> descriptor)
        {
            Name = "ProjectsMutation";
            descriptor.Field(t => t.Create(default(ProjectCreateUpdateModel)))
                .Description("Add a project.")
                .RequirePermission(Activity.UpdateProject);

            descriptor.Field(t => t.Update(default(string), default(ProjectCreateUpdateModel)))
                .Description("Update a project.")
                .RequirePermission(Activity.UpdateProject);

            descriptor.Field(t => t.Remove(default(string)))
                .Description("Permanently remove a project.")
                .RequirePermission(Activity.DeleteProject);
        }
    }
}

/* scaffolding [
    
    {
      "FileName": "DefaultMutation.cs",
      "Indexline": "using SteveTheTradeBot.Api.Components.Projects;",
      "InsertAbove": false,
      "InsertInline": false,
      "Lines": [
        "using SteveTheTradeBot.Api.Components.Projects;"
      ]
    },
    {
      "FileName": "DefaultMutation.cs",
      "Indexline": "ProjectsMutationType",
      "InsertAbove": false,
      "InsertInline": false,
      "Lines": [
        "Field<ProjectsMutationType>(\"projects\", resolve: context => Task.BuildFromHttpContext(new object()));"
      ]
    }
] scaffolding */