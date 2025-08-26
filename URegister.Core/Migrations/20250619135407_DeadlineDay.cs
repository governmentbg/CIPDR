using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class DeadlineDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deadline_days",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    deadline_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Вид срок за изпълнение на услуга"),
                    days = table.Column<int>(type: "integer", nullable: false, comment: "Срок за изпълнение на услуга/дни"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deadline_days", x => x.id);
                },
                comment: "Срокове за изпълнение на услуга");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deadline_days");
        }
    }
}
