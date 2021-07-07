using SteveTheTradeBot.Core.Framework.CommandQuery;

namespace SteveTheTradeBot.Core.Components.Users
{
    public class UserPagedLookupOptions : PagedLookupOptionsBase
    {
        public string Search { get; set; }
        public SortOptions? Sort { get; set; }

        public enum SortOptions
        {
            Name,
            Recent
        }
    }
}