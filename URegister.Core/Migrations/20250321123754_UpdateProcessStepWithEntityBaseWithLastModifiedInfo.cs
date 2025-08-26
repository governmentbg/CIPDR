using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProcessStepWithEntityBaseWithLastModifiedInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_on",
                table: "process_steps",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "process_steps",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Дали записът е активен");

            migrationBuilder.AddColumn<Guid>(
                name: "modified_by_user_id",
                table: "process_steps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на потребителят променил последно записа");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_on",
                table: "process_steps",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(2025, 3, 21, 0, 0, 0, 0, DateTimeKind.Utc),
                comment: "Дата на последна промяна");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_on",
                table: "process_steps");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "process_steps");

            migrationBuilder.DropColumn(
                name: "modified_by_user_id",
                table: "process_steps");

            migrationBuilder.DropColumn(
                name: "modified_on",
                table: "process_steps");
        }
    }
}
