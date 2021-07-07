using System;

namespace SteveTheTradeBot.Dal.Models.Base
{
    public abstract class BaseDalModel : IBaseDalModel
    {
        protected BaseDalModel()
        {
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;
        }

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}