using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class softDeletableDeletedOnMadeTimestampz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "services",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "service_steps",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "registers",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "register_items",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "processes",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "process_steps",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "forms",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "file_metadata",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "custom_views",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "blanks_templates",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "services",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "service_steps",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "registers",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "register_items",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "processes",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "process_steps",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "forms",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "file_metadata",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "custom_views",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "blanks_templates",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");
        }
    }
}
