using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uregister.Users.Migrations
{
    /// <inheritdoc />
    public partial class UserEMailRecive3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "recive_e_form_notification",
                table: "user_e_mail_recives",
                newName: "receive_e_form_notification");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "receive_e_form_notification",
                table: "user_e_mail_recives",
                newName: "recive_e_form_notification");
        }
    }
}
