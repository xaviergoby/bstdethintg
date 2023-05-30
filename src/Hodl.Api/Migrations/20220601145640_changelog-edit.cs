using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hodl.Api.Migrations;

public partial class Changelogedit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ChangeLog_AspNetUsers_UserId",
            table: "ChangeLog");

        migrationBuilder.AlterColumn<DateTime>(
            name: "DateEnd",
            table: "Funds",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AlterColumn<Guid>(
            name: "UserId",
            table: "ChangeLog",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.UpdateData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: new Guid("0aff7b76-053d-43d9-aa1b-308df84c32e1"),
            column: "ConcurrencyStamp",
            value: "affc7507-cd4b-4131-9f11-bd6bb3df15c1");

        migrationBuilder.UpdateData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
            column: "ConcurrencyStamp",
            value: "212f34e5-a2d3-4624-bed2-c3890e015e4c");

        migrationBuilder.UpdateData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
            column: "ConcurrencyStamp",
            value: "623ca578-389d-4ec3-ade4-beb5f5bcfe64");

        migrationBuilder.UpdateData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
            column: "ConcurrencyStamp",
            value: "8262c8e4-d463-448a-8484-d90e8965117c");

        migrationBuilder.UpdateData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
            column: "ConcurrencyStamp",
            value: "37a0554a-dcca-4b24-a155-60eeaea2e346");

        migrationBuilder.UpdateData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
            column: "ConcurrencyStamp",
            value: "864e6ddf-afa1-472d-a410-1b85f49d7b9f");

        migrationBuilder.AddForeignKey(
            name: "FK_ChangeLog_AspNetUsers_UserId",
            table: "ChangeLog",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ChangeLog_AspNetUsers_UserId",
            table: "ChangeLog");

        migrationBuilder.AlterColumn<DateTime>(
            name: "DateEnd",
            table: "Funds",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone",
            oldNullable: true);

        migrationBuilder.AlterColumn<Guid>(
            name: "UserId",
            table: "ChangeLog",
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
            value: "1396354b-0858-418e-bdf6-2ceacd8a028c");

        migrationBuilder.UpdateData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: new Guid("420d65a9-f385-4f19-9c25-b48eaa4ee2dd"),
            column: "ConcurrencyStamp",
            value: "38fd857d-491b-4f66-8106-f570f86cddf2");

        migrationBuilder.UpdateData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: new Guid("526198db-4e9f-4200-bbf9-82ecfe39330c"),
            column: "ConcurrencyStamp",
            value: "af8d460a-87bf-47ad-a118-1bd73e140391");

        migrationBuilder.UpdateData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: new Guid("bc1bd304-deee-4ecb-b00e-420484984c83"),
            column: "ConcurrencyStamp",
            value: "fec9462a-949b-42ee-82bc-7218518d42e2");

        migrationBuilder.UpdateData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: new Guid("c1a6e520-01f1-4fed-85e1-a2a358d7fb02"),
            column: "ConcurrencyStamp",
            value: "430938de-50bd-4155-9ec4-30b885e17c64");

        migrationBuilder.UpdateData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: new Guid("d12fa1e4-409f-42f6-900c-dbd8295ca1cf"),
            column: "ConcurrencyStamp",
            value: "2213cdd1-7e3c-47d1-9b4b-9178c15e418a");

        migrationBuilder.AddForeignKey(
            name: "FK_ChangeLog_AspNetUsers_UserId",
            table: "ChangeLog",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
