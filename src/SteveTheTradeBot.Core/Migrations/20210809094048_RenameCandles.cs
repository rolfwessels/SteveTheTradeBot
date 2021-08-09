using Microsoft.EntityFrameworkCore.Migrations;

namespace SteveTheTradeBot.Core.Migrations
{
    public partial class RenameCandles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TradeOrder",
                newName: "TradeOrders");

            migrationBuilder.RenameTable(
                name: "TradeFeedCandles",
                newName: "TradeQuotes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TradeOrders",
                newName: "TradeOrder");

            migrationBuilder.RenameTable(
                name: "TradeQuotes",
                newName: "TradeFeedCandles");
        }
    }
}
