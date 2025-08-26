using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class FieldAndFieldTypeAreSoftDeletableNow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_on",
                table: "fields",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "fields",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Дали записът е активен");

            migrationBuilder.AlterColumn<bool>(
                name: "is_complex_field",
                table: "field_types",
                type: "boolean",
                nullable: false,
                comment: "Дали полето е сложно",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Дали полето е комплексно");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_on",
                table: "field_types",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "field_types",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Дали записът е активен");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_on",
                table: "fields");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "fields");

            migrationBuilder.DropColumn(
                name: "deleted_on",
                table: "field_types");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "field_types");

            migrationBuilder.AlterColumn<bool>(
                name: "is_complex_field",
                table: "field_types",
                type: "boolean",
                nullable: false,
                comment: "Дали полето е комплексно",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Дали полето е сложно");
        }
    }
}
