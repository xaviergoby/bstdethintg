using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class AddHoldingToListingRefs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeAccounts_ExchangeAccounts_ParentAccountId",
                table: "ExchangeAccounts");

            migrationBuilder.AddColumn<long>(
                name: "CurrencyRateId",
                table: "Holdings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ListingId",
                table: "Holdings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentAccountId",
                table: "ExchangeAccounts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

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
                name: "IX_Holdings_CurrencyRateId",
                table: "Holdings",
                column: "CurrencyRateId");

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_ListingId",
                table: "Holdings",
                column: "ListingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeAccounts_ExchangeAccounts_ParentAccountId",
                table: "ExchangeAccounts",
                column: "ParentAccountId",
                principalTable: "ExchangeAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Holdings_CurrencyRates_CurrencyRateId",
                table: "Holdings",
                column: "CurrencyRateId",
                principalTable: "CurrencyRates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Holdings_Listings_ListingId",
                table: "Holdings",
                column: "ListingId",
                principalTable: "Listings",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeAccounts_ExchangeAccounts_ParentAccountId",
                table: "ExchangeAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Holdings_CurrencyRates_CurrencyRateId",
                table: "Holdings");

            migrationBuilder.DropForeignKey(
                name: "FK_Holdings_Listings_ListingId",
                table: "Holdings");

            migrationBuilder.DropIndex(
                name: "IX_Holdings_CurrencyRateId",
                table: "Holdings");

            migrationBuilder.DropIndex(
                name: "IX_Holdings_ListingId",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "CurrencyRateId",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "ListingId",
                table: "Holdings");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentAccountId",
                table: "ExchangeAccounts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "dc11a65e-ab4b-4fca-a1ec-98c9570dc7bf");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "39f3892d-d6a0-4066-90cf-3919a5932eff");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "53066dee-d92d-4def-8892-e82f89f6619e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "c9b2ca83-973c-4374-9be8-26a03f2b59d5");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "f1a0c7fe-9574-467d-b76a-fd8548015d99");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "59441e0b-b00b-4ee3-8755-32c40104c38a");

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeAccounts_ExchangeAccounts_ParentAccountId",
                table: "ExchangeAccounts",
                column: "ParentAccountId",
                principalTable: "ExchangeAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
