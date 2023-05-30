using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TokenContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CryptoId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockchainNetworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractAddress = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenContracts_BlockchainNetworks_BlockchainNetworkId",
                        column: x => x.BlockchainNetworkId,
                        principalTable: "BlockchainNetworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TokenContracts_CryptoCurrencies_CryptoId",
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
                value: "77300f42-494b-40ab-b6be-134560840b09");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "0ea93274-5a98-4dff-9c4d-a446d3434a87");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "5cd14572-81e5-44c4-9c9f-7f30687cf801");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "418bf213-b119-4367-b75f-78b9b920d519");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "3aabbae8-19bc-4fee-9da3-3ff9bec33b81");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "afe50ced-93fe-4d70-a75f-2f2eb22334be");

            migrationBuilder.CreateIndex(
                name: "IX_TokenContracts_BlockchainNetworkId",
                table: "TokenContracts",
                column: "BlockchainNetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenContracts_CryptoId_BlockchainNetworkId_ContractAddress",
                table: "TokenContracts",
                columns: new[] { "CryptoId", "BlockchainNetworkId", "ContractAddress" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenContracts");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "5ff2a5d2-e3c1-418d-89d6-f416fa2897a4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "a81d0648-6857-4efe-a2d4-0693532e9ced");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "16ca1976-4490-4dfa-ab7f-809d0713aa4c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "66a78e5e-38c5-4fa5-9064-d9579967708f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "258e1ab9-d670-4ecc-817e-527104c2206e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "f1d6b91d-55cd-43b9-ac22-d0418ac85f9f");
        }
    }
}
