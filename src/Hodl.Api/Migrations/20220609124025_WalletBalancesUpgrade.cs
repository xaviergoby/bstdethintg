using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class WalletBalancesUpgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Currencies_CurrencyCode",
                table: "BankAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Wallets_WalletId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_BlockchainNetworks_BlockchainNetworkId",
                table: "Wallets");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_CryptoCurrencies_CryptoId",
                table: "Wallets");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_ExchangeAccounts_ExchangeAccountId",
                table: "Wallets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_BlockchainNetworkId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_CryptoId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Orders_WalletId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_CurrencyCode",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "BlockchainNetworkId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "CryptoId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "TimeStamp",
                table: "BankAccounts");

            migrationBuilder.AlterColumn<Guid>(
                name: "ExchangeAccountId",
                table: "Wallets",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Wallets",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Wallets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WalletAddress",
                table: "Orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FundOwnerId",
                table: "Funds",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Banks",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IBAN",
                table: "BankAccounts",
                type: "character varying(34)",
                maxLength: 34,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BIC",
                table: "BankAccounts",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets",
                column: "Address");

            migrationBuilder.CreateTable(
                name: "BankBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankBalances_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankBalances_Currencies_CurrencyCode",
                        column: x => x.CurrencyCode,
                        principalTable: "Currencies",
                        principalColumn: "ISOCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundOwners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Department = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Country = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundOwners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WalletBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CryptoId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockchainNetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletBalances_BlockchainNetworks_BlockchainNetworkId",
                        column: x => x.BlockchainNetworkId,
                        principalTable: "BlockchainNetworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WalletBalances_CryptoCurrencies_CryptoId",
                        column: x => x.CryptoId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WalletBalances_Wallets_Address",
                        column: x => x.Address,
                        principalTable: "Wallets",
                        principalColumn: "Address");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WalletAddress",
                table: "Orders",
                column: "WalletAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_FundOwnerId",
                table: "Funds",
                column: "FundOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BankBalances_BankAccountId_CurrencyCode_TimeStamp",
                table: "BankBalances",
                columns: new[] { "BankAccountId", "CurrencyCode", "TimeStamp" });

            migrationBuilder.CreateIndex(
                name: "IX_BankBalances_CurrencyCode",
                table: "BankBalances",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_WalletBalances_Address_CryptoId_BlockchainNetworkId",
                table: "WalletBalances",
                columns: new[] { "Address", "CryptoId", "BlockchainNetworkId" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletBalances_BlockchainNetworkId",
                table: "WalletBalances",
                column: "BlockchainNetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletBalances_CryptoId_BlockchainNetworkId",
                table: "WalletBalances",
                columns: new[] { "CryptoId", "BlockchainNetworkId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Funds_FundOwners_FundOwnerId",
                table: "Funds",
                column: "FundOwnerId",
                principalTable: "FundOwners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Wallets_WalletAddress",
                table: "Orders",
                column: "WalletAddress",
                principalTable: "Wallets",
                principalColumn: "Address");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_ExchangeAccounts_ExchangeAccountId",
                table: "Wallets",
                column: "ExchangeAccountId",
                principalTable: "ExchangeAccounts",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Funds_FundOwners_FundOwnerId",
                table: "Funds");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Wallets_WalletAddress",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_ExchangeAccounts_ExchangeAccountId",
                table: "Wallets");

            migrationBuilder.DropTable(
                name: "BankBalances");

            migrationBuilder.DropTable(
                name: "FundOwners");

            migrationBuilder.DropTable(
                name: "WalletBalances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Orders_WalletAddress",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Funds_FundOwnerId",
                table: "Funds");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "WalletAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FundOwnerId",
                table: "Funds");

            migrationBuilder.AlterColumn<Guid>(
                name: "ExchangeAccountId",
                table: "Wallets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Wallets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Wallets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BlockchainNetworkId",
                table: "Wallets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CryptoId",
                table: "Wallets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "Wallets",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "WalletId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Banks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IBAN",
                table: "BankAccounts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(34)",
                oldMaxLength: 34,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BIC",
                table: "BankAccounts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "BankAccounts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "BankAccounts",
                type: "character varying(3)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeStamp",
                table: "BankAccounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "6a169213-aab6-4126-ac94-8227eb20e059");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "6125fbec-a2e7-4619-85bf-a9804ccac475");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "9fc4422e-9c90-445a-a601-1576656754f9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "6ab1b8a0-cea2-4500-90dc-c8e8b273d03a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "c9ddbd02-517d-457f-954f-fe703036c9a7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "a239478c-8664-4ce7-b0fe-87f95e90a3bd");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_BlockchainNetworkId",
                table: "Wallets",
                column: "BlockchainNetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CryptoId",
                table: "Wallets",
                column: "CryptoId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WalletId",
                table: "Orders",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_CurrencyCode",
                table: "BankAccounts",
                column: "CurrencyCode");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Currencies_CurrencyCode",
                table: "BankAccounts",
                column: "CurrencyCode",
                principalTable: "Currencies",
                principalColumn: "ISOCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Wallets_WalletId",
                table: "Orders",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_BlockchainNetworks_BlockchainNetworkId",
                table: "Wallets",
                column: "BlockchainNetworkId",
                principalTable: "BlockchainNetworks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_CryptoCurrencies_CryptoId",
                table: "Wallets",
                column: "CryptoId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_ExchangeAccounts_ExchangeAccountId",
                table: "Wallets",
                column: "ExchangeAccountId",
                principalTable: "ExchangeAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
