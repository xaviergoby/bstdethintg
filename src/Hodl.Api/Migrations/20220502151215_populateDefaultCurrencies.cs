using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class PopulateDefaultCurrencies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundCategory_Categories_CategoryId",
                table: "FundCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_FundCategory_Funds_FundId",
                table: "FundCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FundCategory",
                table: "FundCategory");

            migrationBuilder.RenameTable(
                name: "FundCategory",
                newName: "FundCategories");

            migrationBuilder.RenameIndex(
                name: "IX_FundCategory_FundId_CategoryId",
                table: "FundCategories",
                newName: "IX_FundCategories_FundId_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_FundCategory_CategoryId",
                table: "FundCategories",
                newName: "IX_FundCategories_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FundCategories",
                table: "FundCategories",
                columns: new[] { "FundId", "CategoryId" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "1396354b-0858-418e-bdf6-2ceacd8a028c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "38fd857d-491b-4f66-8106-f570f86cddf2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "af8d460a-87bf-47ad-a118-1bd73e140391");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "fec9462a-949b-42ee-82bc-7218518d42e2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "430938de-50bd-4155-9ec4-30b885e17c64");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "2213cdd1-7e3c-47d1-9b4b-9178c15e418a");

            migrationBuilder.AddForeignKey(
                name: "FK_FundCategories_Categories_CategoryId",
                table: "FundCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FundCategories_Funds_FundId",
                table: "FundCategories",
                column: "FundId",
                principalTable: "Funds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundCategories_Categories_CategoryId",
                table: "FundCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_FundCategories_Funds_FundId",
                table: "FundCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FundCategories",
                table: "FundCategories");

            migrationBuilder.DeleteData(
                table: "CryptoCurrencies",
                keyColumn: "Id",
                keyValue: new Guid("796923d5-a790-40d1-9074-c8fe3c25d049"));

            migrationBuilder.DeleteData(
                table: "CryptoCurrencies",
                keyColumn: "Id",
                keyValue: new Guid("fb3194c2-e732-4219-89b9-bc63eea2a861"));

            migrationBuilder.DeleteData(
                table: "CurrencyRates",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "CurrencyRates",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "CurrencyRates",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "CurrencyRates",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "ISOCode",
                keyValue: "CHF");

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "ISOCode",
                keyValue: "EUR");

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "ISOCode",
                keyValue: "GBP");

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "ISOCode",
                keyValue: "USD");

            migrationBuilder.RenameTable(
                name: "FundCategories",
                newName: "FundCategory");

            migrationBuilder.RenameIndex(
                name: "IX_FundCategories_FundId_CategoryId",
                table: "FundCategory",
                newName: "IX_FundCategory_FundId_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_FundCategories_CategoryId",
                table: "FundCategory",
                newName: "IX_FundCategory_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FundCategory",
                table: "FundCategory",
                columns: new[] { "FundId", "CategoryId" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "95dc3015-4c17-4bc4-a690-0aab9eb9dda3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "a21c186b-b2b5-4fda-9ff2-cc0f4a2849cb");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "a0da5b4b-3735-4d44-9d87-b282d2ab1739");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "2da6ffab-047b-4db2-8215-46a07f3b057e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "485eee96-889c-4c8b-b406-2b517aa55b67");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "a81e8983-0bad-4897-bba1-2facd493c6d8");

            migrationBuilder.AddForeignKey(
                name: "FK_FundCategory_Categories_CategoryId",
                table: "FundCategory",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FundCategory_Funds_FundId",
                table: "FundCategory",
                column: "FundId",
                principalTable: "Funds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
