using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class BlanksTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blanks_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на услуга"),
                    form_parent_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на тип форма"),
                    created_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на създаване"),
                    created_by = table.Column<string>(type: "text", nullable: true, comment: "Създадена от"),
                    content = table.Column<string>(type: "text", nullable: true, comment: "Съдържание на бланка"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на създаване"),
                    modified_by = table.Column<string>(type: "text", nullable: true, comment: "Създадена от")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blanks_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_blanks_templates_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blanks_templates_service_id",
                table: "blanks_templates",
                column: "service_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blanks_templates");
        }
    }
}
