using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessIsSendEMailDeadlineDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_send_e_mail_deadline_date",
                table: "processes",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Нотификация за настъпващ срок за изпълнение на услуга");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_send_e_mail_deadline_date",
                table: "processes");
        }
    }
}
