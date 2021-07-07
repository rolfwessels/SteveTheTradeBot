using System;
using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.MongoDb.Migrations
{
    public class DbVersion : IBaseDalModel
    {
        public DbVersion()
        {
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        #region Implementation of IBaseDalModel

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        #endregion
    }
}