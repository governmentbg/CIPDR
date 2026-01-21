using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uregister.Users.Migrations
{
    /// <inheritdoc />
    public partial class receive_email_on_error : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "receive_email_on_error",
                table: "identity_users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "receive_email_on_error",
                table: "identity_users");
        }
    }
}
