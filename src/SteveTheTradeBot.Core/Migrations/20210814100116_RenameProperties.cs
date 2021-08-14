using Microsoft.EntityFrameworkCore.Migrations;

namespace SteveTheTradeBot.Core.Migrations
{
    public partial class RenameProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Strategies_StrategyInstanceId",
                table: "Properties");
            migrationBuilder.Sql(
                @"UPDATE ""Properties"" SET ""Key"" = 'StopLoss' WHERE  ""Key"" LIKE 'currentStopLoss' ");
            migrationBuilder.Sql(
                @"UPDATE ""Properties"" SET ""Key"" = 'UpdateStopLossAt' WHERE  ""Key"" LIKE 'movePercent' ");
            migrationBuilder.Sql(
                @"UPDATE ""Properties"" SET ""Key"" = 'UpdateStopLossAt' WHERE  ""Key"" LIKE 'MoveProfit' ");
            migrationBuilder.DropPrimaryKey(
                name: "PK_Properties",
                table: "Properties");
            migrationBuilder.RenameTable(
                name: "Properties",
                newName: "StrategyProperties");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StrategyProperties",
                table: "StrategyProperties",
                columns: new[] { "StrategyInstanceId", "Key" });

            migrationBuilder.AddForeignKey(
                name: "FK_StrategyProperties_Strategies_StrategyInstanceId",
                table: "StrategyProperties",
                column: "StrategyInstanceId",
                principalTable: "Strategies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
           
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StrategyProperties_Strategies_StrategyInstanceId",
                table: "StrategyProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StrategyProperties",
                table: "StrategyProperties");

            migrationBuilder.RenameTable(
                name: "StrategyProperties",
                newName: "Properties");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Properties",
                table: "Properties",
                columns: new[] { "StrategyInstanceId", "Key" });

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Strategies_StrategyInstanceId",
                table: "Properties",
                column: "StrategyInstanceId",
                principalTable: "Strategies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
