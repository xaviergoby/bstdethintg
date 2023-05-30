using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class RenameOrderAssets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CryptoCurrencies_FromCryptoId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CryptoCurrencies_ToCryptoId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Pairs_CryptoCurrencies_FromCryptoId",
                table: "Pairs");

            migrationBuilder.DropForeignKey(
                name: "FK_Pairs_CryptoCurrencies_ToCryptoId",
                table: "Pairs");

            migrationBuilder.DropIndex(
                name: "IX_Pairs_FromCryptoId_ToCryptoId",
                table: "Pairs");

            migrationBuilder.DropIndex(
                name: "IX_Pairs_ToCryptoId",
                table: "Pairs");

            migrationBuilder.RenameColumn(
                name: "ToCryptoId",
                table: "Pairs",
                newName: "BaseAssetId");

            migrationBuilder.RenameColumn(
                name: "FromCryptoId",
                table: "Pairs",
                newName: "QuoteAssetId");

            migrationBuilder.RenameColumn(
                name: "ToCryptoId",
                table: "Orders",
                newName: "BaseAssetId");

            migrationBuilder.RenameColumn(
                name: "FromCryptoId",
                table: "Orders",
                newName: "QuoteAssetId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_ToCryptoId_Type_DateTime",
                table: "Orders",
                newName: "IX_Orders_BaseAssetId_Type_DateTime");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_ToCryptoId_State",
                table: "Orders",
                newName: "IX_Orders_BaseAssetId_State");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_FromCryptoId_Type_DateTime",
                table: "Orders",
                newName: "IX_Orders_QuoteAssetId_Type_DateTime");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_FromCryptoId_State",
                table: "Orders",
                newName: "IX_Orders_QuoteAssetId_State");

            migrationBuilder.AddColumn<string>(
                name: "ExchangeTradeId",
                table: "Trades",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExchangeOrderId",
                table: "Orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMaker",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaker",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "9a34b6fd-25cd-4d66-9156-d2a7bce3b0e6");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "15ba0230-6298-4b91-8a52-61881e541132");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "1efcfe02-3bce-4527-bd76-d68c98c3d133");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "196d236d-f200-434a-a785-4dfd74f8dfc2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "08594bbb-1d87-4055-8ea7-6e26e7e63215");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "d73bb06b-2572-488d-877c-dc2b434f11a6");

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_QuoteAssetId",
                table: "Pairs",
                column: "QuoteAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_BaseAssetId_QuoteAssetId",
                table: "Pairs",
                columns: new[] { "BaseAssetId", "QuoteAssetId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_CryptoCurrencies_QuoteAssetId",
                table: "Orders",
                column: "QuoteAssetId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_CryptoCurrencies_BaseAssetId",
                table: "Orders",
                column: "BaseAssetId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pairs_CryptoCurrencies_QuoteAssetId",
                table: "Pairs",
                column: "QuoteAssetId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pairs_CryptoCurrencies_BaseAssetId",
                table: "Pairs",
                column: "BaseAssetId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CryptoCurrencies_QuoteAssetId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CryptoCurrencies_BaseAssetId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Pairs_CryptoCurrencies_QuoteAssetId",
                table: "Pairs");

            migrationBuilder.DropForeignKey(
                name: "FK_Pairs_CryptoCurrencies_BaseAssetId",
                table: "Pairs");

            migrationBuilder.DropIndex(
                name: "IX_Pairs_QuoteAssetId",
                table: "Pairs");

            migrationBuilder.DropIndex(
                name: "IX_Pairs_BaseAssetId_QuoteAssetId",
                table: "Pairs");

            migrationBuilder.DropColumn(
                name: "ExchangeTradeId",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "ExchangeOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsMaker",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsTaker",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "BaseAssetId",
                table: "Pairs",
                newName: "ToCryptoId");

            migrationBuilder.RenameColumn(
                name: "QuoteAssetId",
                table: "Pairs",
                newName: "FromCryptoId");

            migrationBuilder.RenameColumn(
                name: "BaseAssetId",
                table: "Orders",
                newName: "ToCryptoId");

            migrationBuilder.RenameColumn(
                name: "QuoteAssetId",
                table: "Orders",
                newName: "FromCryptoId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_BaseAssetId_Type_DateTime",
                table: "Orders",
                newName: "IX_Orders_ToCryptoId_Type_DateTime");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_BaseAssetId_State",
                table: "Orders",
                newName: "IX_Orders_ToCryptoId_State");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_QuoteAssetId_Type_DateTime",
                table: "Orders",
                newName: "IX_Orders_FromCryptoId_Type_DateTime");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_QuoteAssetId_State",
                table: "Orders",
                newName: "IX_Orders_FromCryptoId_State");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "dbff766b-504d-4072-8e22-c73c3c0b96ac");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "3f83e56a-c092-416b-9d67-d32a85ca1ab7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "251caf95-5d55-425c-b9a1-6e28cf2e2fa4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "51b28689-b8dd-4b64-8436-e4e55d43d3f9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "bc489a84-a65c-4a86-ac75-8b6dd5cce7ef");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "99b96527-1a3f-48ba-8a87-223707bba013");

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_FromCryptoId_ToCryptoId",
                table: "Pairs",
                columns: new[] { "FromCryptoId", "ToCryptoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_ToCryptoId",
                table: "Pairs",
                column: "ToCryptoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_CryptoCurrencies_FromCryptoId",
                table: "Orders",
                column: "FromCryptoId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_CryptoCurrencies_ToCryptoId",
                table: "Orders",
                column: "ToCryptoId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pairs_CryptoCurrencies_FromCryptoId",
                table: "Pairs",
                column: "FromCryptoId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pairs_CryptoCurrencies_ToCryptoId",
                table: "Pairs",
                column: "ToCryptoId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
