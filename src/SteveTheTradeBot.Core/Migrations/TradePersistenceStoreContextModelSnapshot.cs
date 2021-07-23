﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SteveTheTradeBot.Core.Components.Storage;

namespace SteveTheTradeBot.Core.Migrations
{
    [DbContext(typeof(TradePersistenceStoreContext))]
    partial class TradePersistenceStoreContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.7")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("SteveTheTradeBot.Dal.Models.Trades.DynamicPlotter", b =>
                {
                    b.Property<string>("Feed")
                        .HasColumnType("text");

                    b.Property<string>("Label")
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("Value")
                        .HasColumnType("numeric");

                    b.HasKey("Feed", "Label", "Date");

                    b.ToTable("DynamicPlots");
                });

            modelBuilder.Entity("SteveTheTradeBot.Dal.Models.Trades.HistoricalTrade", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CurrencyPair")
                        .HasColumnType("text");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("numeric");

                    b.Property<decimal>("QuoteVolume")
                        .HasColumnType("numeric");

                    b.Property<int>("SequenceId")
                        .HasColumnType("integer");

                    b.Property<string>("TakerSide")
                        .HasColumnType("text");

                    b.Property<DateTime>("TradedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("TradedAt", "SequenceId");

                    b.ToTable("HistoricalTrades");
                });

            modelBuilder.Entity("SteveTheTradeBot.Dal.Models.Trades.SimpleParam", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("Key");

                    b.ToTable("SimpleParam");
                });

            modelBuilder.Entity("SteveTheTradeBot.Dal.Models.Trades.StrategyInstance", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<TimeSpan>("AverageTimeInMarket")
                        .HasColumnType("interval");

                    b.Property<double>("AverageTradesPerMonth")
                        .HasColumnType("double precision");

                    b.Property<decimal>("BaseAmount")
                        .HasColumnType("numeric");

                    b.Property<string>("BaseAmountCurrency")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Feed")
                        .HasColumnType("text");

                    b.Property<decimal>("FirstClose")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("FirstStart")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("InvestmentAmount")
                        .HasColumnType("numeric");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsBackTest")
                        .HasColumnType("boolean");

                    b.Property<decimal>("LargestLoss")
                        .HasColumnType("numeric");

                    b.Property<decimal>("LargestProfit")
                        .HasColumnType("numeric");

                    b.Property<decimal>("LastClose")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("LastDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("NumberOfLosingTrades")
                        .HasColumnType("numeric");

                    b.Property<decimal>("NumberOfProfitableTrades")
                        .HasColumnType("numeric");

                    b.Property<string>("Pair")
                        .HasColumnType("text");

                    b.Property<decimal>("PercentMarketProfit")
                        .HasColumnType("numeric");

                    b.Property<decimal>("PercentOfProfitableTrades")
                        .HasColumnType("numeric");

                    b.Property<decimal>("PercentProfit")
                        .HasColumnType("numeric");

                    b.Property<int>("PeriodSize")
                        .HasColumnType("integer");

                    b.Property<decimal>("QuoteAmount")
                        .HasColumnType("numeric");

                    b.Property<string>("QuoteAmountCurrency")
                        .HasColumnType("text");

                    b.Property<string>("Reference")
                        .HasColumnType("text");

                    b.Property<string>("StrategyName")
                        .HasColumnType("text");

                    b.Property<decimal>("TotalActiveTrades")
                        .HasColumnType("numeric");

                    b.Property<decimal>("TotalFee")
                        .HasColumnType("numeric");

                    b.Property<decimal>("TotalLoss")
                        .HasColumnType("numeric");

                    b.Property<decimal>("TotalNumberOfTrades")
                        .HasColumnType("numeric");

                    b.Property<decimal>("TotalProfit")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("Strategies");
                });

            modelBuilder.Entity("SteveTheTradeBot.Dal.Models.Trades.StrategyTrade", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<decimal>("BuyPrice")
                        .HasColumnType("numeric");

                    b.Property<decimal>("BuyQuantity")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("FeeAmount")
                        .HasColumnType("numeric");

                    b.Property<string>("FeeCurrency")
                        .HasColumnType("text");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<decimal>("Profit")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SellPrice")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SellValue")
                        .HasColumnType("numeric");

                    b.Property<string>("StrategyInstanceId")
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("StrategyInstanceId");

                    b.ToTable("Trades");
                });

            modelBuilder.Entity("SteveTheTradeBot.Dal.Models.Trades.TradeFeedCandle", b =>
                {
                    b.Property<string>("Feed")
                        .HasColumnType("text");

                    b.Property<string>("CurrencyPair")
                        .HasColumnType("text");

                    b.Property<int>("PeriodSize")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("Close")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("High")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Low")
                        .HasColumnType("numeric");

                    b.Property<Dictionary<string, Nullable<decimal>>>("Metric")
                        .HasColumnType("jsonb");

                    b.Property<decimal>("Open")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("Volume")
                        .HasColumnType("numeric");

                    b.HasKey("Feed", "CurrencyPair", "PeriodSize", "Date");

                    b.ToTable("TradeFeedCandles");
                });

            modelBuilder.Entity("SteveTheTradeBot.Dal.Models.Trades.TradeOrder", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("BrokerOrderId")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CurrencyPair")
                        .HasColumnType("text");

                    b.Property<string>("FailedReason")
                        .HasColumnType("text");

                    b.Property<decimal>("FeeAmount")
                        .HasColumnType("numeric");

                    b.Property<string>("FeeCurrency")
                        .HasColumnType("text");

                    b.Property<decimal>("OrderPrice")
                        .HasColumnType("numeric");

                    b.Property<int>("OrderSide")
                        .HasColumnType("integer");

                    b.Property<int>("OrderStatusType")
                        .HasColumnType("integer");

                    b.Property<string>("OrderType")
                        .HasColumnType("text");

                    b.Property<decimal>("OriginalQuantity")
                        .HasColumnType("numeric");

                    b.Property<string>("OutCurrency")
                        .HasColumnType("text");

                    b.Property<decimal>("OutQuantity")
                        .HasColumnType("numeric");

                    b.Property<decimal>("PriceAtRequest")
                        .HasColumnType("numeric");

                    b.Property<decimal>("RemainingQuantity")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("RequestDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("StrategyTradeId")
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdateDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("StrategyTradeId");

                    b.ToTable("TradeOrder");
                });

            modelBuilder.Entity("SteveTheTradeBot.Dal.Models.Trades.StrategyTrade", b =>
                {
                    b.HasOne("SteveTheTradeBot.Dal.Models.Trades.StrategyInstance", null)
                        .WithMany("Trades")
                        .HasForeignKey("StrategyInstanceId");
                });

            modelBuilder.Entity("SteveTheTradeBot.Dal.Models.Trades.TradeOrder", b =>
                {
                    b.HasOne("SteveTheTradeBot.Dal.Models.Trades.StrategyTrade", null)
                        .WithMany("Orders")
                        .HasForeignKey("StrategyTradeId");
                });

            modelBuilder.Entity("SteveTheTradeBot.Dal.Models.Trades.StrategyInstance", b =>
                {
                    b.Navigation("Trades");
                });

            modelBuilder.Entity("SteveTheTradeBot.Dal.Models.Trades.StrategyTrade", b =>
                {
                    b.Navigation("Orders");
                });
#pragma warning restore 612, 618
        }
    }
}
