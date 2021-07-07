using System;

namespace SteveTheTradeBot.Shared.Models.Shared
{
    public abstract class BaseModel : IBaseModel
    {
        public string Id { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}