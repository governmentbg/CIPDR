using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class InstructionResponseReceived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "received_by",
                table: "instruction_responses",
                type: "uuid",
                nullable: true,
                comment: "Прието от");

            migrationBuilder.AddColumn<DateTime>(
                name: "received_on",
                table: "instruction_responses",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на приемане");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "received_by",
                table: "instruction_responses");

            migrationBuilder.DropColumn(
                name: "received_on",
                table: "instruction_responses");
        }
    }
}
