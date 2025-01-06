using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class NomenclatureTypePublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_public",
                table: "nomenclature_types",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Публичен номенклатурен тип");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_public",
                table: "nomenclature_types");
        }
    }
}
