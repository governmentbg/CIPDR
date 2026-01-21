using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uregister.Users.Migrations
{
    /// <inheritdoc />
    public partial class ReceiveInstructionResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "receive_instruction_response",
                table: "user_e_mail_recives",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "receive_instruction_response",
                table: "user_e_mail_recives");
        }
    }
}
