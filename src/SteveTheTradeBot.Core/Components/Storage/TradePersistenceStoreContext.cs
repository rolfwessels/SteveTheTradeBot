using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Base;
using SteveTheTradeBot.Dal.Models.Trades;

namespace SteveTheTradeBot.Core.Components.Storage
{
    public class TradePersistenceStoreContext : DbContext
    {
        private bool _isInMemoryContainer;

        public TradePersistenceStoreContext()
            : base(TradePersistenceFactory.DbContextOptions(Settings.Instance.NpgsqlConnection))
        {
        }

        public TradePersistenceStoreContext(DbContextOptions<TradePersistenceStoreContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<StrategyTrade> Trades { get; set; }
        public DbSet<StrategyInstance> Strategies { get; set; }
        public DbSet<HistoricalTrade> HistoricalTrades { get; set; }
        public DbSet<TradeFeedCandle> TradeFeedCandles { get; set; }
        public DbSet<DynamicPlotter> DynamicPlots { get; set; }
        public DbSet<SimpleParam> SimpleParam { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _isInMemoryContainer = optionsBuilder.Options.Extensions.Select(x => x.GetType().Name).Contains("InMemoryOptionsExtension");
            
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistoricalTrade>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<StrategyInstance>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<StrategyTrade>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<StrategyInstance>()
                .HasMany(p => p.Trades)
                .WithOne();

            modelBuilder.Entity<HistoricalTrade>()
                .HasIndex(b => new { b.TradedAt, b.SequenceId });
            modelBuilder.Entity<TradeFeedCandle>()
                .HasKey(b => new { b.Feed, b.CurrencyPair, b.PeriodSize, b.Date });
            if (_isInMemoryContainer)
            {
                modelBuilder.Entity<TradeFeedCandle>().Ignore(t => t.Metric);
            }
            else
            {
                modelBuilder.Entity<TradeFeedCandle>().Property(x => x.Metric).HasColumnType("jsonb");
            }

            modelBuilder.Entity<DynamicPlotter>()
                .HasKey(b => new { b.Feed, b.Label, b.Date });
            modelBuilder.Entity<SimpleParam>()
                .HasKey(b => b.Key);
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseDalModel && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entityEntryEntity = entityEntry.Entity as BaseDalModel;
                if (entityEntryEntity == null) continue;
                entityEntryEntity.UpdateDate = DateTime.Now;
                if (entityEntry.State == EntityState.Added)
                {
                    entityEntryEntity.CreateDate = DateTime.Now;
                }
            }

            return base.SaveChanges();
        }
    }



}