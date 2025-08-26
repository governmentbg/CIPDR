using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessIncomingDateAndOldIncomingDateAreNowDateWithTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "old_incoming_date",
                table: "processes",
                type: "timestamptz",
                nullable: true,
                comment: "Стара дата на входиране",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true,
                oldComment: "Стара дата на входиране");

            migrationBuilder.AlterColumn<DateTime>(
                name: "incoming_date",
                table: "processes",
                type: "timestamptz",
                nullable: false,
                comment: "Дата на входиране",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldComment: "Дата на входиране");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "old_incoming_date",
                table: "processes",
                type: "date",
                nullable: true,
                comment: "Стара дата на входиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Стара дата на входиране");

            migrationBuilder.AlterColumn<DateTime>(
                name: "incoming_date",
                table: "processes",
                type: "date",
                nullable: false,
                comment: "Дата на входиране",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldComment: "Дата на входиране");
        }
    }
}
