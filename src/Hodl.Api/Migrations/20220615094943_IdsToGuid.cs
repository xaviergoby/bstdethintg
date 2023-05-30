using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class IdsToGuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop tables with LONG id's
            //migrationBuilder.DropIndex(
            //    name: "IX_Transfers_HodlingId",
            //    table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_HoldingId_BookingPeriod_DateTime",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_OppositeTransferId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Trades_ExchangeAccountId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_OrderId_ExchangeAccountId_DateTime",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_OrderFundings_FeeCurrencyId",
                table: "OrderFundings");

            migrationBuilder.DropIndex(
                name: "IX_OrderFundings_FundId",
                table: "OrderFundings");

            migrationBuilder.DropIndex(
                name: "IX_Orders_FeeCurrencyId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PairString_State",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PairString_Type_DateTime",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_WalletAddress",
                table: "Orders");

            migrationBuilder.DropTable("Transfers");
            migrationBuilder.DropTable("Trades");
            migrationBuilder.DropTable("OrderFundings");
            migrationBuilder.DropTable("Orders");

            // Then recreate the tables but now with Guid as identifiers
            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HoldingId = table.Column<Guid>(type: "uuid", nullable: false),
                    OppositeTransferId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingPeriod = table.Column<string>(type: "nchar(6)", maxLength: 6, nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    TransactionSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    TransferAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TransferFee = table.Column<decimal>(type: "numeric", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Holdings_HoldingId",
                        column: x => x.HoldingId,
                        principalTable: "Holdings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transfers_Transfers_OppositeTransferId",
                        column: x => x.OppositeTransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PairString = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Side = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Executed = table.Column<decimal>(type: "numeric", nullable: false),
                    AveragePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalFee = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeCurrencyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_CryptoCurrencies_FeeCurrencyId",
                        column: x => x.FeeCurrencyId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Pairs_PairString",
                        column: x => x.PairString,
                        principalTable: "Pairs",
                        principalColumn: "PairString",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Wallets_WalletAddress",
                        column: x => x.WalletAddress,
                        principalTable: "Wallets",
                        principalColumn: "Address",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderFundings",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingPeriod = table.Column<string>(type: "nchar(6)", maxLength: 6, nullable: false),
                    OrderPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderFee = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeCurrencyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderFundings", x => new { x.OrderId, x.FundId });
                    table.ForeignKey(
                        name: "FK_OrderFundings_CryptoCurrencies_FeeCurrencyId",
                        column: x => x.FeeCurrencyId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderFundings_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderFundings_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Executed = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    Fee = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeCurrency = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trades_ExchangeAccounts_ExchangeAccountId",
                        column: x => x.ExchangeAccountId,
                        principalTable: "ExchangeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trades_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HoldingId",
                table: "Transfers",
                column: "HoldingId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HoldingId_BookingPeriod_DateTime",
                table: "Transfers",
                columns: new[] { "HoldingId", "BookingPeriod", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_OppositeTransferId",
                table: "Transfers",
                column: "OppositeTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_ExchangeAccountId",
                table: "Trades",
                column: "ExchangeAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_OrderId_ExchangeAccountId_DateTime",
                table: "Trades",
                columns: new[] { "OrderId", "ExchangeAccountId", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderFundings_FeeCurrencyId",
                table: "OrderFundings",
                column: "FeeCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderFundings_FundId",
                table: "OrderFundings",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FeeCurrencyId",
                table: "Orders",
                column: "FeeCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PairString_State",
                table: "Orders",
                columns: new[] { "PairString", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PairString_Type_DateTime",
                table: "Orders",
                columns: new[] { "PairString", "Type", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WalletAddress",
                table: "Orders",
                column: "WalletAddress");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "7c8a99d3-2532-448b-b372-307f8c32d11f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "d1d2f141-e00a-416f-9450-c97742f9d58e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "054f1237-4de3-4f02-afb9-036b418a8ab6");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "a9231ef4-1c1e-46f3-88ea-2fc5a469bfb0");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "9117e2f0-d777-4ca9-8f17-568b0c0f9b8e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "bf23238e-2cc8-49f0-86a4-e5b9c42a31c8");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tables with LONG id's
            migrationBuilder.DropIndex(
                name: "IX_Transfers_HoldingId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_HoldingId_BookingPeriod_DateTime",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_OppositeTransferId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Trades_ExchangeAccountId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_OrderId_ExchangeAccountId_DateTime",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_OrderFundings_FeeCurrencyId",
                table: "OrderFundings");

            migrationBuilder.DropIndex(
                name: "IX_OrderFundings_FundId",
                table: "OrderFundings");

            migrationBuilder.DropIndex(
                name: "IX_Orders_FeeCurrencyId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PairString_State",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PairString_Type_DateTime",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_WalletAddress",
                table: "Orders");

            migrationBuilder.DropTable("Transfers");
            migrationBuilder.DropTable("Trades");
            migrationBuilder.DropTable("OrderFundings");
            migrationBuilder.DropTable("Orders");

            // Recreate the tables
            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HoldingId = table.Column<Guid>(type: "uuid", nullable: false),
                    OppositeTransferId = table.Column<long>(type: "bigint", nullable: true),
                    BookingPeriod = table.Column<string>(type: "nchar(6)", maxLength: 6, nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    TransactionSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    TransferAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TransferFee = table.Column<decimal>(type: "numeric", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Holdings_HoldingId",
                        column: x => x.HoldingId,
                        principalTable: "Holdings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transfers_Transfers_OppositeTransferId",
                        column: x => x.OppositeTransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PairString = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Side = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Executed = table.Column<decimal>(type: "numeric", nullable: false),
                    AveragePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalFee = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeCurrencyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_CryptoCurrencies_FeeCurrencyId",
                        column: x => x.FeeCurrencyId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Pairs_PairString",
                        column: x => x.PairString,
                        principalTable: "Pairs",
                        principalColumn: "PairString",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Wallets_WalletAddress",
                        column: x => x.WalletAddress,
                        principalTable: "Wallets",
                        principalColumn: "Address",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderFundings",
                columns: table => new
                {
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingPeriod = table.Column<string>(type: "nchar(6)", maxLength: 6, nullable: false),
                    OrderPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderFee = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeCurrencyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderFundings", x => new { x.OrderId, x.FundId });
                    table.ForeignKey(
                        name: "FK_OrderFundings_CryptoCurrencies_FeeCurrencyId",
                        column: x => x.FeeCurrencyId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderFundings_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderFundings_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ExchangeAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Executed = table.Column<decimal>(type: "numeric", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    Fee = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeCurrency = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trades_ExchangeAccounts_ExchangeAccountId",
                        column: x => x.ExchangeAccountId,
                        principalTable: "ExchangeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trades_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HoldingId",
                table: "Transfers",
                column: "HoldingId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HoldingId_BookingPeriod_DateTime",
                table: "Transfers",
                columns: new[] { "HoldingId", "BookingPeriod", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_OppositeTransferId",
                table: "Transfers",
                column: "OppositeTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_ExchangeAccountId",
                table: "Trades",
                column: "ExchangeAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_OrderId_ExchangeAccountId_DateTime",
                table: "Trades",
                columns: new[] { "OrderId", "ExchangeAccountId", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderFundings_FeeCurrencyId",
                table: "OrderFundings",
                column: "FeeCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderFundings_FundId",
                table: "OrderFundings",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FeeCurrencyId",
                table: "Orders",
                column: "FeeCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PairString_State",
                table: "Orders",
                columns: new[] { "PairString", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PairString_Type_DateTime",
                table: "Orders",
                columns: new[] { "PairString", "Type", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WalletAddress",
                table: "Orders",
                column: "WalletAddress");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "2c9c1921-b533-4863-ba0f-c86f0ff8f6b5");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "8cb1088d-ef4c-4db6-aed5-d93d47d5ac9c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "53be3830-dab8-4d54-999a-ae2d6bbffc14");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "f0848e3b-cb87-46ce-8912-359ec10e72bf");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "f042bdd4-ffec-4e64-80f8-ef6b0d84157a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "23236226-8bb2-4988-81ea-ac9c492b916e");
        }
    }
}
