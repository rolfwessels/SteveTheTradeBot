using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace SteveTheTradeBot.Core.Migrations
{
    public partial class AddParamsAndUpdateDateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "TradeFeedCandles",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: DateTime.Now);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyPair",
                table: "TradeFeedCandles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "TradeFeedCandles",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: DateTime.Now);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "HistoricalTrades",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: DateTime.Now);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "HistoricalTrades",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: DateTime.Now);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "DynamicPlots",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: DateTime.Now);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                table: "DynamicPlots",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: DateTime.Now);

            migrationBuilder.CreateTable(
                name: "SimpleParam",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleParam", x => x.Key);
                });


            migrationBuilder.Sql($"UPDATE public.\"TradeFeedCandles\" SET  \"CurrencyPair\"=\"BTCZAR\"");
            migrationBuilder.Sql($"UPDATE public.\"DynamicPlots\" SET  \"CreateDate\"=NOW(), \"UpdateDate\"=NOW()");
            migrationBuilder.Sql($"UPDATE public.\"TradeFeedCandles\" SET  \"CreateDate\"=NOW(), \"UpdateDate\"=NOW()");
            migrationBuilder.Sql($"UPDATE public.\"HistoricalTrades\" SET  \"CreateDate\"=NOW(), \"UpdateDate\"=NOW()");


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimpleParam");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "TradeFeedCandles");

            migrationBuilder.DropColumn(
                name: "CurrencyPair",
                table: "TradeFeedCandles");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "TradeFeedCandles");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "HistoricalTrades");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "HistoricalTrades");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "DynamicPlots");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                table: "DynamicPlots");
        }
    }
}
