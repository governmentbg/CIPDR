using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RegisterFIleMetaDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codeable_concept_code",
                table: "register_file_metadata",
                type: "text",
                nullable: true,
                comment: "Тип файл");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "register_file_metadata",
                type: "text",
                nullable: true,
                comment: "Описание");

            migrationBuilder.AddColumn<string>(
                name: "nomenclature_type",
                table: "register_file_metadata",
                type: "text",
                nullable: true,
                comment: "Type of NomenclatureType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codeable_concept_code",
                table: "register_file_metadata");

            migrationBuilder.DropColumn(
                name: "description",
                table: "register_file_metadata");

            migrationBuilder.DropColumn(
                name: "nomenclature_type",
                table: "register_file_metadata");
        }
    }
}
