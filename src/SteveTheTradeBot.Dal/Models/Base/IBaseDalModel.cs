using System;

namespace SteveTheTradeBot.Dal.Models.Base
{
    public interface IBaseDalModel
    {
        DateTime CreateDate { get; set; }
        DateTime UpdateDate { get; set; }
    }
}