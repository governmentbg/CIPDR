using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AdministrationEFormCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "e_form_code",
                table: "administration",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Код за връзка с е-форми");

            migrationBuilder.AddColumn<Guid>(
                name: "modified_by_user_id",
                table: "administration",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на потребителят променил последно записа");

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_on",
                table: "administration",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "Дата на последна промяна");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "e_form_code",
                table: "administration");

            migrationBuilder.DropColumn(
                name: "modified_by_user_id",
                table: "administration");

            migrationBuilder.DropColumn(
                name: "modified_on",
                table: "administration");
        }
    }
}
