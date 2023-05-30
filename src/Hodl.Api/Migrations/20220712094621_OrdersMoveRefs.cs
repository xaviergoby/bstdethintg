using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class OrdersMoveRefs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Pairs_PairString",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Trades_ExchangeAccounts_ExchangeAccountId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_ExchangeAccountId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_OrderId_ExchangeAccountId_DateTime",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "ExchangeAccountId",
                table: "Trades");

            migrationBuilder.AlterColumn<string>(
                name: "PairString",
                table: "Orders",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExchangeAccountId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "28256c90-91d5-49c8-b8ad-62cb979e733e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "fbe2dc3f-1997-4b0d-8738-66864eb5cdbc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "221ca64e-4cbe-4f0c-80a4-580522382bb2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "cf521ed5-3cce-4168-ac7b-ec79f7cad7f2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "329213d2-073c-4fb9-875c-2f71ca70da2d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "0601b0ef-6e2a-4053-8086-ce72e123335a");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_OrderId_DateTime",
                table: "Trades",
                columns: new[] { "OrderId", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ExchangeAccountId",
                table: "Orders",
                column: "ExchangeAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderFundings_OrderId_FundId",
                table: "OrderFundings",
                columns: new[] { "OrderId", "FundId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ExchangeAccounts_ExchangeAccountId",
                table: "Orders",
                column: "ExchangeAccountId",
                principalTable: "ExchangeAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Pairs_PairString",
                table: "Orders",
                column: "PairString",
                principalTable: "Pairs",
                principalColumn: "PairString",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ExchangeAccounts_ExchangeAccountId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Pairs_PairString",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Trades_OrderId_DateTime",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ExchangeAccountId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderFundings_OrderId_FundId",
                table: "OrderFundings");

            migrationBuilder.DropColumn(
                name: "ExchangeAccountId",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "ExchangeAccountId",
                table: "Trades",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "PairString",
                table: "Orders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12);

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

            migrationBuilder.CreateIndex(
                name: "IX_Trades_ExchangeAccountId",
                table: "Trades",
                column: "ExchangeAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_OrderId_ExchangeAccountId_DateTime",
                table: "Trades",
                columns: new[] { "OrderId", "ExchangeAccountId", "DateTime" });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Pairs_PairString",
                table: "Orders",
                column: "PairString",
                principalTable: "Pairs",
                principalColumn: "PairString");

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_ExchangeAccounts_ExchangeAccountId",
                table: "Trades",
                column: "ExchangeAccountId",
                principalTable: "ExchangeAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
