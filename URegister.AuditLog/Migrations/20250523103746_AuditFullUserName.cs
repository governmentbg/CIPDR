using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.AuditLog.Migrations
{
    /// <inheritdoc />
    public partial class AuditFullUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "user_full_name",
                table: "audits",
                type: "text",
                nullable: true,
                comment: "Пълно име на потребител");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_full_name",
                table: "audits");
        }
    }
}
