using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class StepServiceTypeAndServiceTypeStepAreSoftDeletableNow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_on",
                table: "steps",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "steps",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Дали записът е активен");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_on",
                table: "service_types",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "service_types",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Дали записът е активен");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_on",
                table: "service_type_steps",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "service_type_steps",
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
                table: "steps");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "steps");

            migrationBuilder.DropColumn(
                name: "deleted_on",
                table: "service_types");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "service_types");

            migrationBuilder.DropColumn(
                name: "deleted_on",
                table: "service_type_steps");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "service_type_steps");
        }
    }
}
