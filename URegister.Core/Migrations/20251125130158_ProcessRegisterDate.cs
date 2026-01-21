using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessRegisterDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "register_date",
                table: "processes",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на вписване");

            migrationBuilder.AddColumn<DateTime>(
                name: "register_init_date",
                table: "processes",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на първоначално вписване");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "register_date",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "register_init_date",
                table: "processes");
        }
    }
}
