using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class TradePersistenceStoreContext : DbContext
    {
        private string _connectionString;

        public TradePersistenceStoreContext(string connection)
        {
            _connectionString = connection;
        }

        public DbSet<HistoricalTrade> HistoricalTrades { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(_connectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistoricalTrade>()
                .HasKey(c => c.Id);
        }
    }

    public class HistoricalTrade
    {
        public string Id { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string CurrencyPair { get; set; }
        public DateTime TradedAt { get; set; }
        public string TakerSide { get; set; }
        public int SequenceId { get; set; }
        public decimal QuoteVolume { get; set; }
    }

    
}