using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class AuditFullUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "register_id",
                table: "audits",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на регистър",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<int>(
                name: "register_id",
                table: "audits",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на регистър");
        }
    }
}
