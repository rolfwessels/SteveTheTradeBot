using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SteveTheTradeBot.Core.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoricalTrades",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrencyPair = table.Column<string>(type: "text", nullable: true),
                    TradedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TakerSide = table.Column<string>(type: "text", nullable: true),
                    SequenceId = table.Column<int>(type: "integer", nullable: false),
                    QuoteVolume = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalTrades", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricalTrades");
        }
    }
}
