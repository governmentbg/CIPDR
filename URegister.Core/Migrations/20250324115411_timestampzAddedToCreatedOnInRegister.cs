using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class timestampzAddedToCreatedOnInRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "created_on",
                table: "registers",
                type: "timestamptz",
                nullable: false,
                comment: "Дата на създаване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Дата на създаване");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "created_on",
                table: "registers",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Дата на създаване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldComment: "Дата на създаване");
        }
    }
}
