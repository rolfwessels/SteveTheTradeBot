using Microsoft.EntityFrameworkCore;
using SteveTheTradeBot.Dal.Models.Trades;

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
}