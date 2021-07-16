using System;

namespace SteveTheTradeBot.Dal.Models.Base
{
    public abstract class BaseDalModelWithId : BaseDalModel, IBaseDalModelWithId
    {
        public virtual string Id { get; set; } 
    }

    public abstract class BaseDalModelWithGuid : BaseDalModelWithId
    {
        public override string Id { get; set; } = Guid.NewGuid().ToString("n");
    }
}