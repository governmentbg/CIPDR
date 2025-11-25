using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class StampitAppId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "app_id",
                table: "registers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "AppId за Stampit");

            migrationBuilder.AddColumn<string>(
                name: "app_secret",
                table: "registers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "AppSecret за Stampit");

            migrationBuilder.AddColumn<DateTime>(
                name: "date_deploy",
                table: "registers",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на старт на deploy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "app_id",
                table: "registers");

            migrationBuilder.DropColumn(
                name: "app_secret",
                table: "registers");

            migrationBuilder.DropColumn(
                name: "date_deploy",
                table: "registers");
        }
    }
}
