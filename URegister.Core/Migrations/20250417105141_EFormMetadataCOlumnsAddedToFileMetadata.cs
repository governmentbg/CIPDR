using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class EFormMetadataCOlumnsAddedToFileMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "e_form_date_of_fill",
                table: "file_metadata",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на попълване на е-формата, ако файлът е заявление подадено чрез е-форма");

            migrationBuilder.AddColumn<Guid>(
                name: "e_form_id",
                table: "file_metadata",
                type: "uuid",
                nullable: true,
                comment: "Идентификатор на е-форма, ако файлът е заявление подадено чрез е-форма");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "e_form_date_of_fill",
                table: "file_metadata");

            migrationBuilder.DropColumn(
                name: "e_form_id",
                table: "file_metadata");
        }
    }
}
