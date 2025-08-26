using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class ServiceRNU : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "register_services",
                columns: table => new
                {
                    register_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на регистър"),
                    service_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на услуга"),
                    rnu = table.Column<string>(type: "text", nullable: true, comment: "Референтен номер на услуга (РНУ)"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_register_services", x => new { x.register_id, x.service_id });
                    table.ForeignKey(
                        name: "fk_register_services_registers_register_id",
                        column: x => x.register_id,
                        principalTable: "registers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Услуга в регистъра");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "register_services");
        }
    }
}
