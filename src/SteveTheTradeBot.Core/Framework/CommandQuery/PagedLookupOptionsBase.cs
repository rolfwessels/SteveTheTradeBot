namespace SteveTheTradeBot.Core.Framework.CommandQuery
{
    public class PagedLookupOptionsBase
    {
        public PagedLookupOptionsBase()
        {
            First = 1000;
        }

        public bool IncludeCount { get; set; }
        public int First { get; set; }
    }
}