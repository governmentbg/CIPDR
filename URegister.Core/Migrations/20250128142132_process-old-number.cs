using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class processoldnumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "old_incoming_date",
                table: "processes",
                type: "date",
                nullable: true,
                comment: "Стара дата на входиране");

            migrationBuilder.AddColumn<string>(
                name: "old_incoming_number",
                table: "processes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                comment: "Стар входящ номер");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "old_incoming_date",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "old_incoming_number",
                table: "processes");
        }
    }
}
