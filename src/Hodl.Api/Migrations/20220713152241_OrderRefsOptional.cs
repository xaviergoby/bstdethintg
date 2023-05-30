using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class OrderRefsOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ExchangeAccounts_ExchangeAccountId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Wallets_WalletAddress",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "WalletAddress",
                table: "Orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ExchangeAccountId",
                table: "Orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "d05980ec-a7a7-4353-966c-aca233ab1a91");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "02393563-e93a-4b0a-9a43-531fe5bb8d33");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "6a456c9e-7203-42b0-bfb9-070dfb3a9709");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "1158f872-a005-402f-a832-cb4038031c14");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "08ae6f83-adfd-43a3-bf47-b723bea311a3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "e70a8fc8-e620-4b0a-814e-fc5d0defbd99");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ExchangeAccounts_ExchangeAccountId",
                table: "Orders",
                column: "ExchangeAccountId",
                principalTable: "ExchangeAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Wallets_WalletAddress",
                table: "Orders",
                column: "WalletAddress",
                principalTable: "Wallets",
                principalColumn: "Address",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ExchangeAccounts_ExchangeAccountId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Wallets_WalletAddress",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "WalletAddress",
                table: "Orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<Guid>(
                name: "ExchangeAccountId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "6293f66b-08a8-4562-bd40-1be2d5fed814");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "d9ad397d-9814-4e70-9366-a905341332fd");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "8c7fb7ad-23a0-4dad-ae48-4dc607a869a3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "7c462da0-c219-4668-8de1-14f526a1def2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "70aeb7d3-f975-4f00-8fdb-6b5aa611fd3c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "07a721af-74be-403e-a860-3d51177f4333");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ExchangeAccounts_ExchangeAccountId",
                table: "Orders",
                column: "ExchangeAccountId",
                principalTable: "ExchangeAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Wallets_WalletAddress",
                table: "Orders",
                column: "WalletAddress",
                principalTable: "Wallets",
                principalColumn: "Address");
        }
    }
}
