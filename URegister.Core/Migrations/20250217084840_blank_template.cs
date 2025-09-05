using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class blank_template : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "modified_by_user_id",
                table: "forms",
                type: "uuid",
                nullable: false,
                comment: "Идентификатор на потребителят променил последно записа",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Идентификатор на потребилет променил последно записа");

            migrationBuilder.AlterColumn<Guid>(
                name: "modified_by_user_id",
                table: "file_metadata",
                type: "uuid",
                nullable: false,
                comment: "Идентификатор на потребителят променил последно записа",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Идентификатор на потребилет променил последно записа");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "blanks_templates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                comment: "Код");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "blanks_templates",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                comment: "Име");

            migrationBuilder.CreateTable(
                name: "field_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "field_templates");

            migrationBuilder.DropColumn(
                name: "code",
                table: "blanks_templates");

            migrationBuilder.DropColumn(
                name: "name",
                table: "blanks_templates");

            migrationBuilder.AlterColumn<Guid>(
                name: "modified_by_user_id",
                table: "forms",
                type: "uuid",
                nullable: false,
                comment: "Идентификатор на потребилет променил последно записа",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Идентификатор на потребителят променил последно записа");

            migrationBuilder.AlterColumn<Guid>(
                name: "modified_by_user_id",
                table: "file_metadata",
                type: "uuid",
                nullable: false,
                comment: "Идентификатор на потребилет променил последно записа",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Идентификатор на потребителят променил последно записа");
        }
    }
}
