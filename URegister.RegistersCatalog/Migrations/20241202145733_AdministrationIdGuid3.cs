using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AdministrationIdGuid3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "started_on",
                table: "registers",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на стартиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Дата на стартиране");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "started_on",
                table: "registers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "Дата на стартиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на стартиране");
        }
    }
}
