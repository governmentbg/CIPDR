using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class IndexNomenclatureType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_nomenclature_type_registers_nomenclature_type_id",
                table: "nomenclature_type_registers");

            migrationBuilder.CreateIndex(
                name: "ix_nomenclature_types_type",
                table: "nomenclature_types",
                column: "type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_nomenclature_type_registers_nomenclature_type_id_administra",
                table: "nomenclature_type_registers",
                columns: new[] { "nomenclature_type_id", "administration_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_nomenclature_types_type",
                table: "nomenclature_types");

            migrationBuilder.DropIndex(
                name: "ix_nomenclature_type_registers_nomenclature_type_id_administra",
                table: "nomenclature_type_registers");

            migrationBuilder.CreateIndex(
                name: "ix_nomenclature_type_registers_nomenclature_type_id",
                table: "nomenclature_type_registers",
                column: "nomenclature_type_id");
        }
    }
}
