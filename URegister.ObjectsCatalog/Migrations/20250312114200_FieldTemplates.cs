using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class FieldTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "field_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    field_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на поле"),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Код"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Име"),
                    content = table.Column<string>(type: "text", nullable: true, comment: "Съдържание на бланка"),
                    created_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на създаване"),
                    created_by = table.Column<string>(type: "text", nullable: true, comment: "Създадена от"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на създаване"),
                    modified_by = table.Column<string>(type: "text", nullable: true, comment: "Създадена от")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_field_templates_fields_field_id",
                        column: x => x.field_id,
                        principalTable: "fields",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_field_templates_field_id",
                table: "field_templates",
                column: "field_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "field_templates");
        }
    }
}
