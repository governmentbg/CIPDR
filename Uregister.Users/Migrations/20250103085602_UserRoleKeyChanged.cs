using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uregister.Users.Migrations
{
    /// <inheritdoc />
    public partial class UserRoleKeyChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_identity_user_roles",
                table: "identity_user_roles");

            migrationBuilder.AddPrimaryKey(
                name: "pk_identity_user_roles",
                table: "identity_user_roles",
                columns: new[] { "user_id", "role_id", "register_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_identity_user_roles",
                table: "identity_user_roles");

            migrationBuilder.AddPrimaryKey(
                name: "pk_identity_user_roles",
                table: "identity_user_roles",
                columns: new[] { "user_id", "role_id" });
        }
    }
}
