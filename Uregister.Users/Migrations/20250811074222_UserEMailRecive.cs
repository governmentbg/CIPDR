using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uregister.Users.Migrations
{
    /// <inheritdoc />
    public partial class UserEMailRecive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_e_mail_recives",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    register_id = table.Column<int>(type: "integer", nullable: false),
                    administration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recive_e_form_notification = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_e_mail_recives", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_e_mail_recives_application_user_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_e_mail_recives_user_id",
                table: "user_e_mail_recives",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_e_mail_recives");
        }
    }
}
