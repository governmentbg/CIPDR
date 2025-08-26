using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class registryItemInheritsSoftDeletable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "register_items");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_on",
                table: "register_items",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "register_items",
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
                table: "register_items");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "register_items");

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "register_items",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Заличено поле");
        }
    }
}
