using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class RemoveWalletFundRef : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankBalances_Currencies_CurrencyCode",
                table: "BankBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletBalances_Wallets_Address",
                table: "WalletBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Funds_FundId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_FundId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_WalletBalances_Address_CryptoId_BlockchainNetworkId",
                table: "WalletBalances");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeAccounts_ExchangeId",
                table: "ExchangeAccounts");

            migrationBuilder.DropColumn(
                name: "FundId",
                table: "Wallets");

            migrationBuilder.RenameColumn(
                name: "CurrencyCode",
                table: "BankBalances",
                newName: "CurrencyISOCode");

            migrationBuilder.RenameIndex(
                name: "IX_BankBalances_CurrencyCode",
                table: "BankBalances",
                newName: "IX_BankBalances_CurrencyISOCode");

            migrationBuilder.RenameIndex(
                name: "IX_BankBalances_BankAccountId_CurrencyCode_TimeStamp",
                table: "BankBalances",
                newName: "IX_BankBalances_BankAccountId_CurrencyISOCode_TimeStamp");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "WalletBalances",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "88761d68-d993-4c77-adbe-d57a0ed0ac19");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "46428206-cde7-4763-884f-62eb34466455");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "38eb0f43-3caf-4601-bb09-60f56c5ab7f7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "916789c6-a9a1-43c8-ad2b-4f9d8a6de0d1");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "85b433da-4831-4714-bef4-245e72184ecb");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "755b2079-1ce3-4e31-bf4d-40167ed1f10b");

            migrationBuilder.CreateIndex(
                name: "IX_WalletBalances_Address_CryptoId_BlockchainNetworkId",
                table: "WalletBalances",
                columns: new[] { "Address", "CryptoId", "BlockchainNetworkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeAccounts_ExchangeId_AccountKey",
                table: "ExchangeAccounts",
                columns: new[] { "ExchangeId", "AccountKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankBalances_BankAccountId_CurrencyISOCode",
                table: "BankBalances",
                columns: new[] { "BankAccountId", "CurrencyISOCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BankBalances_Currencies_CurrencyISOCode",
                table: "BankBalances",
                column: "CurrencyISOCode",
                principalTable: "Currencies",
                principalColumn: "ISOCode",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletBalances_Wallets_Address",
                table: "WalletBalances",
                column: "Address",
                principalTable: "Wallets",
                principalColumn: "Address",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankBalances_Currencies_CurrencyISOCode",
                table: "BankBalances");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletBalances_Wallets_Address",
                table: "WalletBalances");

            migrationBuilder.DropIndex(
                name: "IX_WalletBalances_Address_CryptoId_BlockchainNetworkId",
                table: "WalletBalances");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeAccounts_ExchangeId_AccountKey",
                table: "ExchangeAccounts");

            migrationBuilder.DropIndex(
                name: "IX_BankBalances_BankAccountId_CurrencyISOCode",
                table: "BankBalances");

            migrationBuilder.RenameColumn(
                name: "CurrencyISOCode",
                table: "BankBalances",
                newName: "CurrencyCode");

            migrationBuilder.RenameIndex(
                name: "IX_BankBalances_CurrencyISOCode",
                table: "BankBalances",
                newName: "IX_BankBalances_CurrencyCode");

            migrationBuilder.RenameIndex(
                name: "IX_BankBalances_BankAccountId_CurrencyISOCode_TimeStamp",
                table: "BankBalances",
                newName: "IX_BankBalances_BankAccountId_CurrencyCode_TimeStamp");

            migrationBuilder.AddColumn<Guid>(
                name: "FundId",
                table: "Wallets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "WalletBalances",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "3005b3d4-52ae-45eb-b17a-fea18dac2770");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "23cec26a-b42c-4d3b-94a9-05f0b13b8d3c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "00492b7e-55d9-4500-bd58-4691334a5dac");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "7c43fb28-5efc-4d11-8006-b2ecfe28937b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "ad745293-fdde-4632-b523-51aa56f57127");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "48c7b1cb-8ded-43b7-ba14-c8ced32bcb55");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_FundId",
                table: "Wallets",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletBalances_Address_CryptoId_BlockchainNetworkId",
                table: "WalletBalances",
                columns: new[] { "Address", "CryptoId", "BlockchainNetworkId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeAccounts_ExchangeId",
                table: "ExchangeAccounts",
                column: "ExchangeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankBalances_Currencies_CurrencyCode",
                table: "BankBalances",
                column: "CurrencyCode",
                principalTable: "Currencies",
                principalColumn: "ISOCode",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletBalances_Wallets_Address",
                table: "WalletBalances",
                column: "Address",
                principalTable: "Wallets",
                principalColumn: "Address");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Funds_FundId",
                table: "Wallets",
                column: "FundId",
                principalTable: "Funds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
