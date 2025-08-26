using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class fileMetaDataApplicationSubmition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "application_submission",
                table: "e_delivery_file_metadata",
                type: "jsonb",
                nullable: true,
                comment: "Информация от пдф json_submission");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "application_submission",
                table: "e_delivery_file_metadata");
        }
    }
}
