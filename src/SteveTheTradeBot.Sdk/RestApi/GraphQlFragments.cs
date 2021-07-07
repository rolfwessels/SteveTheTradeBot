namespace SteveTheTradeBot.Sdk.RestApi
{
    public class GraphQlFragments
    {
        public static string User = @"fragment userData on User {
                id,
                name,
                email,
                image,
                roles,
                activities,
                createDate,
                updateDate
            } ";

        public const string Project = @"fragment projectData on Project {
              id
              name
            } ";

        public const string CommandResult = @"fragment commandResultData on CommandResult {
              id
              correlationId
            } ";
    }
}