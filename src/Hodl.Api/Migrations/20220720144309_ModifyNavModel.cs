using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    public partial class ModifyNavModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyNavs");

            migrationBuilder.CreateTable(
                name: "Navs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BookingPeriod = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    TotalValue = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalShares = table.Column<int>(type: "integer", nullable: false),
                    ShareHWM = table.Column<decimal>(type: "numeric", nullable: false),
                    ShareGross = table.Column<decimal>(type: "numeric", nullable: false),
                    ShareNAV = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Navs_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "2ef1b8f8-4c78-42c5-a6c0-6c422db26360");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "90215f8f-e5c2-4ab8-a78c-18c24dc96768");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "1aef8978-15c3-4289-a7a7-f0bb4033ba7c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "79313fcd-f110-4033-995c-a0905fbcb753");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "792d733e-82f5-4076-8a22-7d81ccfdd8a4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "92a989a7-6180-4d44-b264-d2ddfb267ed4");

            migrationBuilder.CreateIndex(
                name: "IX_Navs_FundId_DateTime",
                table: "Navs",
                columns: new[] { "FundId", "DateTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Navs");

            migrationBuilder.CreateTable(
                name: "DailyNavs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShareGross = table.Column<decimal>(type: "numeric", nullable: false),
                    ShareHWM = table.Column<decimal>(type: "numeric", nullable: false),
                    ShareNAV = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalShares = table.Column<int>(type: "integer", nullable: false),
                    TotalValue = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyNavs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyNavs_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_DailyNavs_FundId_DateTime",
                table: "DailyNavs",
                columns: new[] { "FundId", "DateTime" });
        }
    }
}
