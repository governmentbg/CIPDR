using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class integrationDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_on",
                table: "e_delivery_messages",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "e_delivery_messages",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Дали записът е активен");

            migrationBuilder.AddColumn<Guid>(
                name: "modified_by_user_id",
                table: "e_delivery_messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на потребителят променил последно записа");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_on",
                table: "e_delivery_messages",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "Дата на последна промяна");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_on",
                table: "e_delivery_file_metadata",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "e_delivery_file_metadata",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Дали записът е активен");

            migrationBuilder.AddColumn<Guid>(
                name: "modified_by_user_id",
                table: "e_delivery_file_metadata",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на потребителят променил последно записа");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_on",
                table: "e_delivery_file_metadata",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "Дата на последна промяна");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_on",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "modified_by_user_id",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "modified_on",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "deleted_on",
                table: "e_delivery_file_metadata");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "e_delivery_file_metadata");

            migrationBuilder.DropColumn(
                name: "modified_by_user_id",
                table: "e_delivery_file_metadata");

            migrationBuilder.DropColumn(
                name: "modified_on",
                table: "e_delivery_file_metadata");
        }
    }
}
