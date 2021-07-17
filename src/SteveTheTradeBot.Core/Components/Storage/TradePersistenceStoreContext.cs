using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class TradePersistenceStoreContext : DbContext
    {
        public TradePersistenceStoreContext() 
            : base(TradePersistenceFactory.DbContextOptions(Settings.Instance.NpgsqlConnection))
        {
        }

        public TradePersistenceStoreContext(DbContextOptions<TradePersistenceStoreContext> dbContextOptions) 
            : base(dbContextOptions)
        {
        }

        public DbSet<HistoricalTrade> HistoricalTrades { get; set; }
        public DbSet<TradeFeedCandle> TradeFeedCandles { get; set; }
        public DbSet<DynamicPlotter> DynamicPlots { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistoricalTrade>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<HistoricalTrade>()
                .HasIndex(b => new {b.TradedAt, b.SequenceId});
            modelBuilder.Entity<TradeFeedCandle>()
                .HasKey(b => new { b.Feed, b.PeriodSize, b.Date });
            modelBuilder.Entity<DynamicPlotter>()
                .HasKey(b => new { b.Feed, b.Label, b.Date });
        }
    }


    
}