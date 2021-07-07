using System.Collections.Generic;

namespace SteveTheTradeBot.Shared.Models.Shared
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int Count { get; set; }
    }
}