using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class EDeleiveryFileMetadataRNU : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "rnu",
                table: "e_delivery_file_metadata",
                type: "text",
                nullable: true,
                comment: "Референтен номер на услуга (РНУ)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rnu",
                table: "e_delivery_file_metadata");
        }
    }
}
