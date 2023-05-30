using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class CascadingDeleteOnOrderFundings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefi",
                table: "Exchanges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "3add24f5-a7d8-4575-a58f-2e24f94e210d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "16a34a93-26b2-4802-bce3-9bafefe29606");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "60765d36-6f03-40fc-b69c-2494c553fe21");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "d5b0ae69-db52-4c68-8cf4-717d33665106");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "578e2b57-1f50-4474-a562-6eaa6fa8763d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "208628d8-ae53-4b17-8902-ae8bf12a5eb8");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefi",
                table: "Exchanges");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "9c8fbd75-23b8-4928-bce7-e50e4ab5563f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "cf716388-027d-405d-bf31-8eec4b9710b2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "b0eaa3e2-2b3a-4b71-82f4-6a1e979dee98");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "0e50a118-3072-49bc-8c24-72df4fbe270c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "e7c61a92-3c8c-40e4-b756-3a477bf5fd8b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "4e2d55c8-2355-442b-ab5b-e2e3bb09c980");
        }
    }
}
