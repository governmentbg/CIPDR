using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class NomenclatureConfirmed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "confirmed",
                table: "codeable_concepts",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Одобрение");

            migrationBuilder.AddColumn<string>(
                name: "confirmed_by",
                table: "codeable_concepts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                comment: "Одобрен от");

            migrationBuilder.AddColumn<DateTime>(
                name: "confirmed_on",
                table: "codeable_concepts",
                type: "timestamptz",
                nullable: true,
                comment: "Дата и час на одобрение");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confirmed",
                table: "codeable_concepts");

            migrationBuilder.DropColumn(
                name: "confirmed_by",
                table: "codeable_concepts");

            migrationBuilder.DropColumn(
                name: "confirmed_on",
                table: "codeable_concepts");
        }
    }
}
