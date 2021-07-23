using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SteveTheTradeBot.Core.Migrations
{
    public partial class AddStrategy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Strategies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    Feed = table.Column<string>(type: "text", nullable: true),
                    Pair = table.Column<string>(type: "text", nullable: true),
                    PeriodSize = table.Column<int>(type: "integer", nullable: false),
                    StrategyName = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsBackTest = table.Column<bool>(type: "boolean", nullable: false),
                    InvestmentAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    BaseAmountCurrency = table.Column<string>(type: "text", nullable: true),
                    QuoteAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    QuoteAmountCurrency = table.Column<string>(type: "text", nullable: true),
                    TotalActiveTrades = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalNumberOfTrades = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageTradesPerMonth = table.Column<double>(type: "double precision", nullable: false),
                    PercentOfProfitableTrades = table.Column<decimal>(type: "numeric", nullable: false),
                    NumberOfProfitableTrades = table.Column<decimal>(type: "numeric", nullable: false),
                    NumberOfLosingTrades = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalProfit = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalLoss = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalFee = table.Column<decimal>(type: "numeric", nullable: false),
                    PercentProfit = table.Column<decimal>(type: "numeric", nullable: false),
                    LargestProfit = table.Column<decimal>(type: "numeric", nullable: false),
                    LargestLoss = table.Column<decimal>(type: "numeric", nullable: false),
                    FirstClose = table.Column<decimal>(type: "numeric", nullable: false),
                    LastClose = table.Column<decimal>(type: "numeric", nullable: false),
                    PercentMarketProfit = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageTimeInMarket = table.Column<TimeSpan>(type: "interval", nullable: false),
                    FirstStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    StrategyInstanceId = table.Column<string>(type: "text", nullable: true),
                    BuyQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    BuyPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    SellValue = table.Column<decimal>(type: "numeric", nullable: false),
                    SellPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Profit = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeCurrency = table.Column<string>(type: "text", nullable: true),
                    FeeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trades_Strategies_StrategyInstanceId",
                        column: x => x.StrategyInstanceId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TradeOrder",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OrderStatusType = table.Column<int>(type: "integer", nullable: false),
                    CurrencyPair = table.Column<string>(type: "text", nullable: true),
                    OrderPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    RemainingQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    OriginalQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderSide = table.Column<int>(type: "integer", nullable: false),
                    OrderType = table.Column<string>(type: "text", nullable: true),
                    BrokerOrderId = table.Column<string>(type: "text", nullable: true),
                    FailedReason = table.Column<string>(type: "text", nullable: true),
                    PriceAtRequest = table.Column<decimal>(type: "numeric", nullable: false),
                    OutQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    OutCurrency = table.Column<string>(type: "text", nullable: true),
                    FeeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeCurrency = table.Column<string>(type: "text", nullable: true),
                    StrategyTradeId = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeOrder_Trades_StrategyTradeId",
                        column: x => x.StrategyTradeId,
                        principalTable: "Trades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TradeOrder_StrategyTradeId",
                table: "TradeOrder",
                column: "StrategyTradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_StrategyInstanceId",
                table: "Trades",
                column: "StrategyInstanceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TradeOrder");

            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "Strategies");
        }
    }
}
