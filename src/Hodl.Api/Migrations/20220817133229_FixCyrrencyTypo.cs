using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class FixCyrrencyTypo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CryptoCategory_Categories_CategoryId",
                table: "CryptoCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_CryptoCategory_CryptoCurrencies_CryptoId",
                table: "CryptoCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrencyRates_Currencies_CyrrencyISOCode",
                table: "CurrencyRates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CryptoCategory",
                table: "CryptoCategory");

            migrationBuilder.RenameTable(
                name: "CryptoCategory",
                newName: "CryptoCategories");

            migrationBuilder.RenameColumn(
                name: "CyrrencyISOCode",
                table: "CurrencyRates",
                newName: "CurrencyISOCode");

            migrationBuilder.RenameIndex(
                name: "IX_CurrencyRates_CyrrencyISOCode_TimeStamp",
                table: "CurrencyRates",
                newName: "IX_CurrencyRates_CurrencyISOCode_TimeStamp");

            migrationBuilder.RenameIndex(
                name: "IX_CryptoCategory_CryptoId_CategoryId",
                table: "CryptoCategories",
                newName: "IX_CryptoCategories_CryptoId_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CryptoCategories",
                table: "CryptoCategories",
                columns: new[] { "CategoryId", "CryptoId" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "2b4c6d28-27dc-4d35-876e-1f94036f038d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "f2b95cc1-5c78-40bd-870e-00b2efdcdf3a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "832f3ecf-4625-480f-acae-8b1b3b698ac7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "e7f6ccfd-b29a-4c96-a9d0-c38398dcc998");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "9ea9fa78-7c47-4a4c-ab77-1843dc47dbbc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "b9f7a46e-7292-4a0a-88fb-6e158d013cc7");

            migrationBuilder.AddForeignKey(
                name: "FK_CryptoCategories_Categories_CategoryId",
                table: "CryptoCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CryptoCategories_CryptoCurrencies_CryptoId",
                table: "CryptoCategories",
                column: "CryptoId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CurrencyRates_Currencies_CurrencyISOCode",
                table: "CurrencyRates",
                column: "CurrencyISOCode",
                principalTable: "Currencies",
                principalColumn: "ISOCode",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CryptoCategories_Categories_CategoryId",
                table: "CryptoCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_CryptoCategories_CryptoCurrencies_CryptoId",
                table: "CryptoCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrencyRates_Currencies_CurrencyISOCode",
                table: "CurrencyRates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CryptoCategories",
                table: "CryptoCategories");

            migrationBuilder.RenameTable(
                name: "CryptoCategories",
                newName: "CryptoCategory");

            migrationBuilder.RenameColumn(
                name: "CurrencyISOCode",
                table: "CurrencyRates",
                newName: "CyrrencyISOCode");

            migrationBuilder.RenameIndex(
                name: "IX_CurrencyRates_CurrencyISOCode_TimeStamp",
                table: "CurrencyRates",
                newName: "IX_CurrencyRates_CyrrencyISOCode_TimeStamp");

            migrationBuilder.RenameIndex(
                name: "IX_CryptoCategories_CryptoId_CategoryId",
                table: "CryptoCategory",
                newName: "IX_CryptoCategory_CryptoId_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CryptoCategory",
                table: "CryptoCategory",
                columns: new[] { "CategoryId", "CryptoId" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "3d86565a-30d1-4dd8-b060-e6abbf4afac2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "6ae572b9-454d-46e6-84c1-b7b497663103");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "5736d5e5-83cd-4138-8d1b-7171574f2187");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "2b3099f3-63a0-4e5f-998c-c1c17fb59d70");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "196776dc-9bdc-4651-be5c-de60a9498414");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "facb2e56-c293-439e-9418-d1f23c553ae5");

            migrationBuilder.AddForeignKey(
                name: "FK_CryptoCategory_Categories_CategoryId",
                table: "CryptoCategory",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CryptoCategory_CryptoCurrencies_CryptoId",
                table: "CryptoCategory",
                column: "CryptoId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CurrencyRates_Currencies_CyrrencyISOCode",
                table: "CurrencyRates",
                column: "CyrrencyISOCode",
                principalTable: "Currencies",
                principalColumn: "ISOCode",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
