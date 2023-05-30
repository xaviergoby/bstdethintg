using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFundSharesHolding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SharesFundId",
                table: "Holdings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SharesFundIdFundId",
                table: "Holdings",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "ec93c7f1-0ec1-40d9-926c-a573666866eb");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "c5d8c71e-3662-4ef6-a2f7-f31973b10a78");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "747746a4-c3d0-4f0f-82c0-faeb4dd920f1");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "de5ca179-4306-4e02-97a3-0404c34b5d4b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "fb243f17-d457-4e74-9943-5e4e15b47eab");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "bb2a41fd-9299-4704-beda-9d7b55fc6b8a");

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_SharesFundIdFundId",
                table: "Holdings",
                column: "SharesFundIdFundId");

            migrationBuilder.AddForeignKey(
                name: "FK_Holdings_Funds_SharesFundIdFundId",
                table: "Holdings",
                column: "SharesFundIdFundId",
                principalTable: "Funds",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Holdings_Funds_SharesFundIdFundId",
                table: "Holdings");

            migrationBuilder.DropIndex(
                name: "IX_Holdings_SharesFundIdFundId",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "SharesFundId",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "SharesFundIdFundId",
                table: "Holdings");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
                column: "ConcurrencyStamp",
                value: "9f94facb-a848-4a3e-ba57-d584b599558d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
                column: "ConcurrencyStamp",
                value: "b743455f-b53a-4220-9ade-0b59d642eb04");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
                column: "ConcurrencyStamp",
                value: "7347e5eb-27e2-4f65-a5b5-6fff56651c1e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
                column: "ConcurrencyStamp",
                value: "fd508273-b6ae-4b93-a177-1e5a7e05066d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
                column: "ConcurrencyStamp",
                value: "b2a66728-7206-4f0e-a293-c9eba72ae1fe");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
                column: "ConcurrencyStamp",
                value: "4fe2d9b4-f7e1-4b83-ab67-a100ca83336a");
        }
    }
}
