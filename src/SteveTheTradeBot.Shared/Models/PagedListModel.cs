using System.Collections.Generic;

namespace SteveTheTradeBot.Shared.Models
{
    public class PagedListModel<T>
    {
        public long Count { get; set; }

        public List<T> Items { get; set; }
    }
}