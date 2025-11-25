using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class formConditionsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "form_conditions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Индентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    form_parent_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на първата версия на формата"),
                    triggering_field_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Име на полето активиращо условие"),
                    triggering_nomenclature_value = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Код на номенклатура активираща условие"),
                    fields_to_hide = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Полета за скриване"),
                    fields_to_show = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Полета за показване"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_form_conditions", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "form_conditions");
        }
    }
}
