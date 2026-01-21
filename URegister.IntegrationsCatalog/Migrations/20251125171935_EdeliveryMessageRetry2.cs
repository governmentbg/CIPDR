using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class EdeliveryMessageRetry2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "file_name",
                table: "e_delivery_messages",
                type: "text",
                nullable: true,
                comment: "Име на файл");

            migrationBuilder.AddColumn<string>(
                name: "file_url",
                table: "e_delivery_messages",
                type: "text",
                nullable: true,
                comment: "URL на файл");

            migrationBuilder.AddColumn<int>(
                name: "profile_id",
                table: "e_delivery_messages",
                type: "integer",
                nullable: true,
                comment: "Профил ИД в ССЕВ");

            migrationBuilder.AddColumn<int>(
                name: "template_id",
                table: "e_delivery_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Темплейт на съобщение");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_name",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "file_url",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "profile_id",
                table: "e_delivery_messages");

            migrationBuilder.DropColumn(
                name: "template_id",
                table: "e_delivery_messages");
        }
    }
}
