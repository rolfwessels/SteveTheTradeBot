using SteveTheTradeBot.Dal.Models.Projects;
using SteveTheTradeBot.Shared.Models.Projects;
using HotChocolate.Types;

namespace SteveTheTradeBot.Api.Components.Projects
{
    public class ProjectType : ObjectType<Project>
    {
        protected override void Configure(IObjectTypeDescriptor<Project> descriptor)
        {
            Name = "Project";
            descriptor.Field(d => d.Id)
                .Type<NonNullType<StringType>>()
                .Description("The id of the project.");
            descriptor.Field(d => d.Name)
                .Type<NonNullType<StringType>>()
                .Description("The name of the project.");
            descriptor.Field(d => d.UpdateDate)
                .Type<NonNullType<DateTimeType>>()
                .Description("The last updated date for the project.");
            descriptor.Field(d => d.CreateDate)
                .Type<NonNullType<DateTimeType>>()
                .Description("The date when the project was created.");
        }
    }
}