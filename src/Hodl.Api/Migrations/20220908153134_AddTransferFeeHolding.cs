using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class AddTransferFeeHolding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Holdings_HoldingId",
                table: "Transfers");

            migrationBuilder.AddColumn<Guid>(
                name: "FeeHoldingId",
                table: "Transfers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(@"UPDATE ""Transfers"" SET ""FeeHoldingId"" = ""HoldingId""");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "0339ef3b-cace-43a0-a0e7-e23a8ce14207");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "c18bf01e-e7e0-4b1d-97a3-c50cd38caf58");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "123b8157-b2f2-4184-8f2e-7e2ed255c484");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "f29c2ab0-d7f4-4c67-bad5-45a30b8194e8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "ef097985-277a-4825-b4a5-bb23ce13424b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "ebc8c450-69db-4177-b01f-c46701df1ca1");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_FeeHoldingId",
                table: "Transfers",
                column: "FeeHoldingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Holdings_FeeHoldingId",
                table: "Transfers",
                column: "FeeHoldingId",
                principalTable: "Holdings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Holdings_HoldingId",
                table: "Transfers",
                column: "HoldingId",
                principalTable: "Holdings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Holdings_FeeHoldingId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Holdings_HoldingId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_FeeHoldingId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "FeeHoldingId",
                table: "Transfers");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "e28a0c33-2e17-4c09-8653-f2567cd92f2f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "81a0f156-8b4c-4f27-ba1d-4d665239f10d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "6f0393c3-a554-4b80-89fe-15324cce460e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "fce78c4f-b7a1-4d75-a88e-3692a917e74f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "4909d132-9202-4026-8f36-5d5ff182bab2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "872271ef-ed2d-4a15-9db0-17e5e5ca4dd8");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Holdings_HoldingId",
                table: "Transfers",
                column: "HoldingId",
                principalTable: "Holdings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
