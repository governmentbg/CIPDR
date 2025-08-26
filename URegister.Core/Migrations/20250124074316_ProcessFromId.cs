using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessFromId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "from_process_id",
                table: "processes",
                type: "uuid",
                nullable: true,
                comment: "Идентификатор на първоначален процес");

            migrationBuilder.AddColumn<long>(
                name: "order_number",
                table: "processes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Поредност на вписването");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "from_process_id",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "order_number",
                table: "processes");
        }
    }
}
