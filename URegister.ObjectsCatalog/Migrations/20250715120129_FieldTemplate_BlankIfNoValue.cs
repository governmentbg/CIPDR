using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class FieldTemplate_BlankIfNoValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "code",
                table: "field_templates");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "steps",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "service_types",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "service_type_steps",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "fields",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "field_types",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "field_templates",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AddColumn<bool>(
                name: "blank_if_no_value",
                table: "field_templates",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Празен резултат ако няма стойност за полето");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "blank_if_no_value",
                table: "field_templates");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "steps",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "service_types",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "service_type_steps",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "fields",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "field_types",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "field_templates",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "field_templates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                comment: "Код");
        }
    }
}
