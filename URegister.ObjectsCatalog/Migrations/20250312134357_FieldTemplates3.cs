using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class FieldTemplates3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "field_templates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "field_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    field_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на поле"),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Код"),
                    content = table.Column<string>(type: "text", nullable: true, comment: "Съдържание на бланка"),
                    created_by = table.Column<string>(type: "text", nullable: true, comment: "Създадена от"),
                    created_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на създаване"),
                    modified_by = table.Column<string>(type: "text", nullable: true, comment: "Създадена от"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на създаване"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Име")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_field_templates_field_types_field_type_id",
                        column: x => x.field_type_id,
                        principalTable: "field_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_field_templates_field_type_id",
                table: "field_templates",
                column: "field_type_id");
        }
    }
}
