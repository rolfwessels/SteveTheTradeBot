using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SteveTheTradeBot.Core.Migrations
{
    public partial class AddMetricMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TradeFeedCandles",
                table: "TradeFeedCandles");

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyPair",
                table: "TradeFeedCandles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Dictionary<string, Nullable<decimal>>>(
                name: "Metric",
                table: "TradeFeedCandles",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TradeFeedCandles",
                table: "TradeFeedCandles",
                columns: new[] { "Feed", "CurrencyPair", "PeriodSize", "Date" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TradeFeedCandles",
                table: "TradeFeedCandles");

            migrationBuilder.DropColumn(
                name: "Metric",
                table: "TradeFeedCandles");

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyPair",
                table: "TradeFeedCandles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TradeFeedCandles",
                table: "TradeFeedCandles",
                columns: new[] { "Feed", "PeriodSize", "Date" });
        }
    }
}
