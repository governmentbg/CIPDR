using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class NomenclatureStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "confirmed_on",
                table: "codeable_concepts",
                newName: "status_on");

            migrationBuilder.RenameColumn(
                name: "confirmed_by",
                table: "codeable_concepts",
                newName: "status_by");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "codeable_concepts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Одобрение");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "codeable_concepts");

            migrationBuilder.RenameColumn(
                name: "status_on",
                table: "codeable_concepts",
                newName: "confirmed_on");

            migrationBuilder.RenameColumn(
                name: "status_by",
                table: "codeable_concepts",
                newName: "confirmed_by");
        }
    }
}
