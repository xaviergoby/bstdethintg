using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class CascadingDeletesOnCryptos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Pairs_PairString",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "CryptoCategories");

            migrationBuilder.DropIndex(
                name: "IX_Pairs_FromCryptoId",
                table: "Pairs");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PairString_State",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PairString_Type_DateTime",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FundCategories",
                table: "FundCategories");

            migrationBuilder.DropIndex(
                name: "IX_FundCategories_CategoryId",
                table: "FundCategories");

            migrationBuilder.DropColumn(
                name: "PairString",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "FromCryptoId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ToCryptoId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_FundCategories",
                table: "FundCategories",
                columns: new[] { "CategoryId", "FundId" });

            migrationBuilder.CreateTable(
                name: "CryptoCategory",
                columns: table => new
                {
                    CryptoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoCategory", x => new { x.CategoryId, x.CryptoId });
                    table.ForeignKey(
                        name: "FK_CryptoCategory_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CryptoCategory_CryptoCurrencies_CryptoId",
                        column: x => x.CryptoId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Pairs_FromCryptoId_ToCryptoId",
                table: "Pairs",
                columns: new[] { "FromCryptoId", "ToCryptoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FromCryptoId_State",
                table: "Orders",
                columns: new[] { "FromCryptoId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FromCryptoId_Type_DateTime",
                table: "Orders",
                columns: new[] { "FromCryptoId", "Type", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ToCryptoId_State",
                table: "Orders",
                columns: new[] { "ToCryptoId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ToCryptoId_Type_DateTime",
                table: "Orders",
                columns: new[] { "ToCryptoId", "Type", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_CryptoCategory_CryptoId_CategoryId",
                table: "CryptoCategory",
                columns: new[] { "CryptoId", "CategoryId" },
                unique: true);

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CryptoCurrencies_FromCryptoId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CryptoCurrencies_ToCryptoId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "CryptoCategory");

            migrationBuilder.DropIndex(
                name: "IX_Pairs_FromCryptoId_ToCryptoId",
                table: "Pairs");

            migrationBuilder.DropIndex(
                name: "IX_Orders_FromCryptoId_State",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_FromCryptoId_Type_DateTime",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ToCryptoId_State",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ToCryptoId_Type_DateTime",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FundCategories",
                table: "FundCategories");

            migrationBuilder.DropColumn(
                name: "FromCryptoId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ToCryptoId",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "PairString",
                table: "Orders",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FundCategories",
                table: "FundCategories",
                columns: new[] { "FundId", "CategoryId" });

            migrationBuilder.CreateTable(
                name: "CryptoCategories",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CryptoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoCategories", x => new { x.CategoryId, x.CryptoId });
                    table.ForeignKey(
                        name: "FK_CryptoCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CryptoCategories_CryptoCurrencies_CryptoId",
                        column: x => x.CryptoId,
                        principalTable: "CryptoCurrencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_Pairs_FromCryptoId",
                table: "Pairs",
                column: "FromCryptoId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PairString_State",
                table: "Orders",
                columns: new[] { "PairString", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PairString_Type_DateTime",
                table: "Orders",
                columns: new[] { "PairString", "Type", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_FundCategories_CategoryId",
                table: "FundCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoCategories_CryptoId",
                table: "CryptoCategories",
                column: "CryptoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Pairs_PairString",
                table: "Orders",
                column: "PairString",
                principalTable: "Pairs",
                principalColumn: "PairString",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
