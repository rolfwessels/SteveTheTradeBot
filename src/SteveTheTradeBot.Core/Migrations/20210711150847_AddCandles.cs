using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SteveTheTradeBot.Core.Migrations
{
    public partial class AddCandles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TradeFeedCandles",
                columns: table => new
                {
                    Feed = table.Column<string>(type: "text", nullable: false),
                    PeriodSize = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Open = table.Column<decimal>(type: "numeric", nullable: false),
                    High = table.Column<decimal>(type: "numeric", nullable: false),
                    Low = table.Column<decimal>(type: "numeric", nullable: false),
                    Close = table.Column<decimal>(type: "numeric", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeFeedCandles", x => new { x.Feed, x.PeriodSize, x.Date });
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalTrades_TradedAt_SequenceId",
                table: "HistoricalTrades",
                columns: new[] { "TradedAt", "SequenceId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TradeFeedCandles");

            migrationBuilder.DropIndex(
                name: "IX_HistoricalTrades_TradedAt_SequenceId",
                table: "HistoricalTrades");
        }
    }
}
