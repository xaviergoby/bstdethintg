using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    /// <inheritdoc />
    public partial class OrderAvgPriceExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AvgPriceCryptoId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AvgPriceRate",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "AvgPriceSource",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "4203f45a-130e-4f61-bd9d-203a598af352");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "795c1d39-1282-4608-a927-42b6485dd4d9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "fedbca2b-0150-49f0-ad69-df999d18f562");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "c7345086-d388-4db1-885e-fae312b6829b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "d3e7747d-a496-474e-be7b-6270ce02e4ec");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "dda4aba3-d49c-48cf-a05f-15159a873bb1");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AvgPriceCryptoId",
                table: "Orders",
                column: "AvgPriceCryptoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_CryptoCurrencies_AvgPriceCryptoId",
                table: "Orders",
                column: "AvgPriceCryptoId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CryptoCurrencies_AvgPriceCryptoId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AvgPriceCryptoId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AvgPriceCryptoId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AvgPriceRate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AvgPriceSource",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "1129d32b-9ad1-4fe5-b320-e002c1be0cec");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "9083f603-7d44-4004-9eac-1ae0010eb470");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "c218242a-fd14-4791-85f0-dbc6ff5f1f0a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "4983b06a-14d8-4d62-923e-2c9aab19f75b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "8cf92e9d-89d3-4bbf-b426-b8343b08d8d3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "9cdddb77-f23b-404e-b188-1b01ca44768a");
        }
    }
}
