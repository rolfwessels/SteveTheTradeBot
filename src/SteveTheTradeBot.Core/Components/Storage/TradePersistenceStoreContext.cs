using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

        public DbSet<TradeOrder> TradeOrders { get; set; }
        public DbSet<StrategyTrade> Trades { get; set; }
        public DbSet<StrategyInstance> Strategies { get; set; }
        public DbSet<HistoricalTrade> HistoricalTrades { get; set; }
        public DbSet<TradeQuote> TradeQuotes { get; set; }
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
            modelBuilder.Entity<TradeOrder>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<StrategyInstance>()
                .HasMany(p => p.Trades)
                .WithOne();
            modelBuilder.Entity<HistoricalTrade>()
                .HasIndex(b => new { b.TradedAt, b.SequenceId });
            modelBuilder.Entity<TradeQuote>()
                .HasKey(b => new { b.Feed, b.CurrencyPair, b.PeriodSize, b.Date });

            if (_isInMemoryContainer)
            {
                modelBuilder.Entity<TradeQuote>().Ignore(t => t.Metric);
            }
            else
            {
                modelBuilder.Entity<TradeQuote>().Property(x => x.Metric).HasColumnType("jsonb");
            }

            modelBuilder.Entity<DynamicPlotter>()
                .HasKey(b => new { b.Feed, b.Label, b.Date });
            modelBuilder.Entity<SimpleParam>()
                .HasKey(b => b.Key);
            modelBuilder.Entity<StrategyInstance.Properties>()
                .HasKey(b => new {b.StrategyInstanceId, b.Key});
            EnsureAllDatesAreUtc(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        private static void EnsureAllDatesAreUtc(ModelBuilder modelBuilder)
        {
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.IsKeyless)
                {
                    continue;
                }

                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
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