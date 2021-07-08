using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class TradePersistenceStoreContext : DbContext
    {
        public TradePersistenceStoreContext(DbContextOptions<TradePersistenceStoreContext> dbContextOptions) 
            : base(dbContextOptions)
        {
        }

        public DbSet<HistoricalTrade> HistoricalTrades { get; set; }

      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistoricalTrade>()
                .HasKey(c => c.Id);
        }
    }
}