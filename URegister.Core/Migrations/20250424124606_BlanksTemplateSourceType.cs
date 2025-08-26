using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class BlanksTemplateSourceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blanks_templates_services_service_id",
                table: "blanks_templates");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "blanks_templates");

            migrationBuilder.AlterColumn<int>(
                name: "service_id",
                table: "blanks_templates",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на услуга",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на услуга");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_on",
                table: "blanks_templates",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "Дата на последна промяна",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на създаване");

            migrationBuilder.AddColumn<int>(
                name: "source_type",
                table: "blanks_templates",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на услуга");

            migrationBuilder.AddForeignKey(
                name: "fk_blanks_templates_services_service_id",
                table: "blanks_templates",
                column: "service_id",
                principalTable: "services",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blanks_templates_services_service_id",
                table: "blanks_templates");

            migrationBuilder.DropColumn(
                name: "source_type",
                table: "blanks_templates");

            migrationBuilder.AlterColumn<int>(
                name: "service_id",
                table: "blanks_templates",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на услуга",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на услуга");

            migrationBuilder.AlterColumn<DateTime>(
                name: "modified_on",
                table: "blanks_templates",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на създаване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldComment: "Дата на последна промяна");

            migrationBuilder.AddColumn<string>(
                name: "modified_by",
                table: "blanks_templates",
                type: "text",
                nullable: true,
                comment: "Създадена от");

            migrationBuilder.AddForeignKey(
                name: "fk_blanks_templates_services_service_id",
                table: "blanks_templates",
                column: "service_id",
                principalTable: "services",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
