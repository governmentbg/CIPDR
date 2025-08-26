using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProcessWithEntityBaseWithLastModifiedInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "modified_by_user_id",
                table: "processes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на потребителят променил последно записа");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_on",
                table: "processes",
                type: "timestamptz",
                nullable: false,
                defaultValue: DateTime.UtcNow,
                comment: "Дата на последна промяна");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "modified_by_user_id",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "modified_on",
                table: "processes");
        }
    }
}
