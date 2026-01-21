using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class EdeliveryMessageRetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "error_count_send",
                table: "e_delivery_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Брой повторения при грешно изпращане");

            migrationBuilder.AddColumn<string>(
                name: "message_text",
                table: "e_delivery_messages",
                type: "text",
                nullable: true,
                comment: "Текст на съобщението при изпращане");

            migrationBuilder.AddColumn<string>(
                name: "subject_text",
                table: "e_delivery_messages",
                type: "text",
                nullable: true,
                comment: "Subject на съобщението при изпращане");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "error_count_send",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "message_text",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "subject_text",
                table: "e_delivery_messages");
        }
    }
}
