using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class UpdateBankDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Holdings_HoldingId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "BIC",
                table: "BankAccounts");

            migrationBuilder.AddColumn<Guid>(
                name: "HodlingId",
                table: "Transfers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Banks",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BIC",
                table: "Banks",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "Banks",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Banks",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Banks",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Zipcode",
                table: "Banks",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IBAN",
                table: "BankAccounts",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(34)",
                oldMaxLength: 34,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "5a31f9af-f58b-43d1-821c-23048b349480");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "ff6c800c-5009-49db-acc7-d278b25a8ae0");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "dd593e3c-5797-41a8-b82b-4aaa283a0ddc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "ad6fba21-c58b-4b1a-a987-097e747b57af");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "7bc625d5-2770-4a16-978f-580022215449");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "39593285-7bf9-4e2d-9ef8-1f21fed405ba");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HodlingId",
                table: "Transfers",
                column: "HodlingId");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_BIC",
                table: "Banks",
                column: "BIC",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Holdings_HodlingId",
                table: "Transfers",
                column: "HodlingId",
                principalTable: "Holdings",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Holdings_HodlingId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_HodlingId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Banks_BIC",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "HodlingId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "BIC",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "Zipcode",
                table: "Banks");

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "Banks",
                type: "character varying(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "IBAN",
                table: "BankAccounts",
                type: "character varying(34)",
                maxLength: 34,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(34)",
                oldMaxLength: 34);

            migrationBuilder.AddColumn<string>(
                name: "BIC",
                table: "BankAccounts",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Holdings_HoldingId",
                table: "Transfers",
                column: "HoldingId",
                principalTable: "Holdings",
                principalColumn: "Id");
        }
    }
}
