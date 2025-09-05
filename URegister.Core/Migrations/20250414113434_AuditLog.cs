using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class AuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на запис"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на потребител"),
                    activity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Идентификатор на операция"),
                    activity_tags = table.Column<string>(type: "jsonb", nullable: true, comment: "Допълнителна информация към операцията"),
                    assembly_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Модул, в който е възникнала операцията"),
                    controller = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Контролер на операцията"),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Действие на операцията"),
                    method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Тип на действието"),
                    parameters = table.Column<string>(type: "jsonb", nullable: true, comment: "Параметри на операцията"),
                    post_data = table.Column<string>(type: "jsonb", nullable: true, comment: "Параметри на post заявката"),
                    type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Тип на операцията"),
                    created = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата и час на събитието (UTC)"),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: false, comment: "IP Адрес на потребителя")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audits", x => x.id);
                },
                comment: "Одитен лог");

            migrationBuilder.CreateTable(
                name: "audit_entity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на запис"),
                    audit_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на запис заявка"),
                    table_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Име на обект"),
                    old_values = table.Column<string>(type: "jsonb", nullable: true, comment: "Стойности преди операцията"),
                    new_values = table.Column<string>(type: "jsonb", nullable: true, comment: "Стойности след операцията"),
                    affected_columns = table.Column<string>(type: "jsonb", nullable: true, comment: "Засегнати данни"),
                    primary_key = table.Column<string>(type: "jsonb", nullable: true, comment: "Идентификатор на обект")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_entity", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_entity_audits_audit_id",
                        column: x => x.audit_id,
                        principalTable: "audits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Одитен лог записи в таблица ");

            migrationBuilder.CreateIndex(
                name: "ix_audit_entity_audit_id",
                table: "audit_entity",
                column: "audit_id");

            migrationBuilder.CreateIndex(
                name: "ix_audits_created",
                table: "audits",
                column: "created");

            migrationBuilder.CreateIndex(
                name: "ix_audits_user_id",
                table: "audits",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_entity");

            migrationBuilder.DropTable(
                name: "audits");
        }
    }
}
