using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class registeritemNomenclatureType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "field_templates");

            migrationBuilder.AddColumn<string>(
                name: "nomenclature_type",
                table: "register_items",
                type: "text",
                nullable: true,
                comment: "Тип номенклатура");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nomenclature_type",
                table: "register_items");

            migrationBuilder.CreateTable(
                name: "field_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Код"),
                    content = table.Column<string>(type: "text", nullable: true, comment: "Съдържание на бланка"),
                    created_by = table.Column<string>(type: "text", nullable: true, comment: "Създадена от"),
                    created_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на създаване"),
                    field_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на поле"),
                    modified_by = table.Column<string>(type: "text", nullable: true, comment: "Създадена от"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на създаване"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Име")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_field_templates", x => x.id);
                });
        }
    }
}
