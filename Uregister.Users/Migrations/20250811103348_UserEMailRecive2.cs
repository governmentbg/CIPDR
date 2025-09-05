using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uregister.Users.Migrations
{
    /// <inheritdoc />
    public partial class UserEMailRecive2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "register_id",
                table: "user_e_mail_recives");

            migrationBuilder.AddColumn<string>(
                name: "register_code",
                table: "user_e_mail_recives",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "register_code",
                table: "user_e_mail_recives");

            migrationBuilder.AddColumn<int>(
                name: "register_id",
                table: "user_e_mail_recives",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
