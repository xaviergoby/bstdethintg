using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class PopulateRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"), "95dc3015-4c17-4bc4-a690-0aab9eb9dda3", "HeadSales", "HEADSALES" },
                    { new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"), "a21c186b-b2b5-4fda-9ff2-cc0f4a2849cb", "Client", "CLIENT" },
                    { new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"), "a0da5b4b-3735-4d44-9d87-b282d2ab1739", "Trader", "TRADER" },
                    { new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"), "2da6ffab-047b-4db2-8215-46a07f3b057e", "Sales", "SALES" },
                    { new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"), "485eee96-889c-4c8b-b406-2b517aa55b67", "Admin", "ADMIN" },
                    { new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"), "a81e8983-0bad-4897-bba1-2facd493c6d8", "LeadTrader", "LEADTRADER" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"));
        }
    }
}
