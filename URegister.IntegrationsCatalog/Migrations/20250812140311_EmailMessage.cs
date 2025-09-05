using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class EmailMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "e_mail_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    status_id = table.Column<int>(type: "integer", nullable: false, comment: "Статус"),
                    error_message = table.Column<string>(type: "text", nullable: true, comment: "Грешка"),
                    error_count = table.Column<int>(type: "integer", nullable: false, comment: "Брой грешки"),
                    source_type = table.Column<int>(type: "integer", nullable: false, comment: "Вид връзка"),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на връзка"),
                    message = table.Column<string>(type: "text", nullable: false, comment: "Съобщение"),
                    subject = table.Column<string>(type: "text", nullable: false),
                    e_mail = table.Column<string>(type: "text", nullable: false, comment: "е-маил адрес"),
                    person_name = table.Column<string>(type: "text", nullable: true, comment: "Получател"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_e_mail_messages", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "e_mail_messages");
        }
    }
}
