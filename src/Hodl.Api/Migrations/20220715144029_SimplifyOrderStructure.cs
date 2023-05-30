using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class SimplifyOrderStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderFundings_CryptoCurrencies_FeeCurrencyId",
                table: "OrderFundings");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CryptoCurrencies_FeeCurrencyId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_FeeCurrencyId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderFundings_FeeCurrencyId",
                table: "OrderFundings");

            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Executed",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FeeCurrencyId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TotalFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FeeCurrencyId",
                table: "OrderFundings");

            migrationBuilder.DropColumn(
                name: "OrderFee",
                table: "OrderFundings");

            migrationBuilder.RenameColumn(
                name: "Side",
                table: "Orders",
                newName: "Direction");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "281d6a5a-7416-4537-b8de-148514fa9e1c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "3f452736-d8b5-427e-85bb-91804aab5809");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "4396b42a-5050-4aa7-881e-383e3d2d9f51");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "bcefe7dc-ac2f-47b5-82ee-a5cb98afb8fe");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "6ec13459-f334-4c0b-ba8b-cb7f4514806e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "37dfd656-aac4-4c87-a1e1-4d6cba406e36");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Direction",
                table: "Orders",
                newName: "Side");

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Executed",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "FeeCurrencyId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFee",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "FeeCurrencyId",
                table: "OrderFundings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "OrderFee",
                table: "OrderFundings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "217aecb1-a44e-4018-aa32-2475bbb03765");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "29790500-bcf7-4c2b-b8b6-794c0fb15834");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "b9c6d3e5-be1a-4417-9300-b14b38016bc9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "5373a870-9347-4f3d-8344-2028c4734051");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "fa9e229c-7fbe-4849-8110-c71f2ad6dfc7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "87dcc742-2114-4e94-9575-33efdb102fc0");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FeeCurrencyId",
                table: "Orders",
                column: "FeeCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderFundings_FeeCurrencyId",
                table: "OrderFundings",
                column: "FeeCurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderFundings_CryptoCurrencies_FeeCurrencyId",
                table: "OrderFundings",
                column: "FeeCurrencyId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_CryptoCurrencies_FeeCurrencyId",
                table: "Orders",
                column: "FeeCurrencyId",
                principalTable: "CryptoCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
