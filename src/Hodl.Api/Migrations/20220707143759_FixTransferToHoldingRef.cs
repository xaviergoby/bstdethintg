using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class FixTransferToHoldingRef : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Holdings_HodlingId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_HodlingId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "HodlingId",
                table: "Transfers");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "b02d8f82-d27d-437d-9ccc-dcf0ca1b00ee");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "8d0b4cc3-658c-4f90-8bfd-c1090ba8234b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "ee3eb665-5286-445e-ae82-dc8158740e37");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "dcfa1138-251a-4400-9d94-3ed06981e791");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "b4421d52-7ce9-459b-8e5f-b37a78e27145");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "755a293e-9b1f-421c-a161-504b496bee2a");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Holdings_HoldingId",
                table: "Transfers",
                column: "HoldingId",
                principalTable: "Holdings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Holdings_HoldingId",
                table: "Transfers");

            migrationBuilder.AddColumn<Guid>(
                name: "HodlingId",
                table: "Transfers",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "fec98a73-a38d-46b7-b892-e5ad6e454b0c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "ca02e7d3-8aef-445c-85c6-00e43a0e0f7a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "087cae7d-0e56-4111-b299-1ce678f5dfe5");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "47e61ac0-a243-40cf-90cc-c3e627105f7b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "c2ed8588-a1b0-49aa-89f0-b5287704daee");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "9f8edc6a-9acf-4a61-91d9-576d9b6cc87c");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HodlingId",
                table: "Transfers",
                column: "HodlingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Holdings_HodlingId",
                table: "Transfers",
                column: "HodlingId",
                principalTable: "Holdings",
                principalColumn: "Id");
        }
    }
}
