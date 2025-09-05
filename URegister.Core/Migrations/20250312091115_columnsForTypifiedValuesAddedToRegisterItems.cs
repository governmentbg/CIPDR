using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class columnsForTypifiedValuesAddedToRegisterItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "bool_value",
                table: "register_items",
                type: "boolean",
                nullable: true,
                comment: "Булева стойност");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_time_value",
                table: "register_items",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата стойност");

            migrationBuilder.AddColumn<decimal>(
                name: "decimal_value",
                table: "register_items",
                type: "numeric",
                nullable: true,
                comment: "Числова стойност");

            migrationBuilder.AddColumn<int>(
                name: "field_type_id",
                table: "register_items",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Тип на поле");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bool_value",
                table: "register_items");

            migrationBuilder.DropColumn(
                name: "date_time_value",
                table: "register_items");

            migrationBuilder.DropColumn(
                name: "decimal_value",
                table: "register_items");

            migrationBuilder.DropColumn(
                name: "field_type_id",
                table: "register_items");
        }
    }
}
