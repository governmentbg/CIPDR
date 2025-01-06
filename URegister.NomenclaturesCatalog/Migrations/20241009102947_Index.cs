using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_codeable_concept_registers_nomenclature_id",
                table: "codeable_concept_registers");

            migrationBuilder.CreateIndex(
                name: "ix_codeable_concept_registers_nomenclature_id_administration_id",
                table: "codeable_concept_registers",
                columns: new[] { "nomenclature_id", "administration_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_codeable_concept_registers_nomenclature_id_administration_id",
                table: "codeable_concept_registers");

            migrationBuilder.CreateIndex(
                name: "ix_codeable_concept_registers_nomenclature_id",
                table: "codeable_concept_registers",
                column: "nomenclature_id");
        }
    }
}
