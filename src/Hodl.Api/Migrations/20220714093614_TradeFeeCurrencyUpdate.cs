using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class TradeFeeCurrencyUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeeCurrency",
                table: "Trades");

            migrationBuilder.AddColumn<Guid>(
                name: "FeeCurrencyId",
                table: "Trades",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "dfc0bf8f-444a-454d-8102-d96eb680ba82");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "4ff7e69b-c27d-4be4-ae5d-8bb427da22b1");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "ef9c3015-5c1f-416c-b068-394f618aac58");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "45da9b4f-6e6b-43a7-9bb0-6d97d41eb82d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "281c4a36-2e84-4318-a7f6-de5b76293f7b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "0697e54f-f326-44f0-919e-4934ac3c9137");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_FeeCurrencyId",
                table: "Trades",
                column: "FeeCurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_CryptoCurrencies_FeeCurrencyId",
                table: "Trades",
                column: "FeeCurrencyId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trades_CryptoCurrencies_FeeCurrencyId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_FeeCurrencyId",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "FeeCurrencyId",
                table: "Trades");

            migrationBuilder.AddColumn<string>(
                name: "FeeCurrency",
                table: "Trades",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

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
    }
}
