using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessFrom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_on",
                table: "processes",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "processes",
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
                table: "processes");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "processes");
        }
    }
}
