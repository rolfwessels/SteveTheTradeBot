using System;

namespace SteveTheTradeBot.Shared.Models.Shared
{
    public abstract class BaseModel : IBaseModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime UpdateDate { get; set; } = DateTime.Now;
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}