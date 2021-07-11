using System;
using Microsoft.EntityFrameworkCore;
using Skender.Stock.Indicators;
using SteveTheTradeBot.Core.Components.Broker;
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
        public DbSet<TradeFeedCandle> TradeFeedCandles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistoricalTrade>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<HistoricalTrade>()
                .HasIndex(b => new {b.TradedAt, b.SequenceId});
            modelBuilder.Entity<TradeFeedCandle>()
                .HasKey(b => new { b.Feed, b.PeriodSize, b.Date });
        }
    }


    
}