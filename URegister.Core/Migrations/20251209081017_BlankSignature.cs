using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class BlankSignature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_register_number",
                table: "blanks_templates",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Генериране на регистров номер за бланката");

            migrationBuilder.CreateTable(
                name: "blank_signature",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Индентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    blank_template_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на бланка"),
                    sign_by_operator = table.Column<bool>(type: "boolean", nullable: false, comment: "Подписва се от обработващия служител"),
                    role_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на роля")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blank_signature", x => x.id);
                    table.ForeignKey(
                        name: "fk_blank_signature_blanks_templates_blank_template_id",
                        column: x => x.blank_template_id,
                        principalTable: "blanks_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Поредност на подписване на бланка");

            migrationBuilder.CreateIndex(
                name: "ix_blank_signature_blank_template_id",
                table: "blank_signature",
                column: "blank_template_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blank_signature");

            migrationBuilder.DropColumn(
                name: "has_register_number",
                table: "blanks_templates");
        }
    }
}
