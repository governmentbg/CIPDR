using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class RegisterItem_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "index",
                table: "register_items",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Индекс на повтарящо се поле");

            migrationBuilder.CreateTable(
                name: "step_role",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Индентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_step_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на стъпка"),
                    role_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на роля"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_step_role", x => x.id);
                    table.ForeignKey(
                        name: "fk_step_role_service_steps_service_step_id",
                        column: x => x.service_step_id,
                        principalTable: "service_steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Стъпка от услуга в регистъра");

            migrationBuilder.CreateIndex(
                name: "ix_step_role_service_step_id",
                table: "step_role",
                column: "service_step_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "step_role");

            migrationBuilder.DropColumn(
                name: "index",
                table: "register_items");
        }
    }
}
