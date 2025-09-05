using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class FieldTemplates2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_field_templates_fields_field_id",
                table: "field_templates");

            migrationBuilder.DropIndex(
                name: "ix_field_templates_field_id",
                table: "field_templates");

            migrationBuilder.DropColumn(
                name: "field_id",
                table: "field_templates");

            migrationBuilder.AddColumn<int>(
                name: "field_type_id",
                table: "field_templates",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на поле");

            migrationBuilder.CreateIndex(
                name: "ix_field_templates_field_type_id",
                table: "field_templates",
                column: "field_type_id");

            migrationBuilder.AddForeignKey(
                name: "fk_field_templates_field_types_field_type_id",
                table: "field_templates",
                column: "field_type_id",
                principalTable: "field_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_field_templates_field_types_field_type_id",
                table: "field_templates");

            migrationBuilder.DropIndex(
                name: "ix_field_templates_field_type_id",
                table: "field_templates");

            migrationBuilder.DropColumn(
                name: "field_type_id",
                table: "field_templates");

            migrationBuilder.AddColumn<Guid>(
                name: "field_id",
                table: "field_templates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на поле");

            migrationBuilder.CreateIndex(
                name: "ix_field_templates_field_id",
                table: "field_templates",
                column: "field_id");

            migrationBuilder.AddForeignKey(
                name: "fk_field_templates_fields_field_id",
                table: "field_templates",
                column: "field_id",
                principalTable: "fields",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
