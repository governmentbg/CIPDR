using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class FileMetaData3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "application_json",
                table: "e_delivery_file_metadata",
                type: "jsonb",
                nullable: true,
                comment: "Информация от пдф",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldComment: "Информация от пдф");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "application_json",
                table: "e_delivery_file_metadata",
                type: "jsonb",
                nullable: false,
                defaultValue: "",
                comment: "Информация от пдф",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true,
                oldComment: "Информация от пдф");
        }
    }
}
