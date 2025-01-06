using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class additionalcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_additional_columns_codeable_concepts_nomenclature_id",
                table: "additional_columns");

            migrationBuilder.DropIndex(
                name: "ix_additional_columns_nomenclature_id",
                table: "additional_columns");

            migrationBuilder.RenameColumn(
                name: "nomenclature_id",
                table: "additional_columns",
                newName: "codeable_concept_id");

            migrationBuilder.CreateIndex(
                name: "ix_additional_columns_codeable_concept_id_name",
                table: "additional_columns",
                columns: new[] { "codeable_concept_id", "name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_additional_columns_codeable_concepts_codeable_concept_id",
                table: "additional_columns",
                column: "codeable_concept_id",
                principalTable: "codeable_concepts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_additional_columns_codeable_concepts_codeable_concept_id",
                table: "additional_columns");

            migrationBuilder.DropIndex(
                name: "ix_additional_columns_codeable_concept_id_name",
                table: "additional_columns");

            migrationBuilder.RenameColumn(
                name: "codeable_concept_id",
                table: "additional_columns",
                newName: "nomenclature_id");

            migrationBuilder.CreateIndex(
                name: "ix_additional_columns_nomenclature_id",
                table: "additional_columns",
                column: "nomenclature_id");

            migrationBuilder.AddForeignKey(
                name: "fk_additional_columns_codeable_concepts_nomenclature_id",
                table: "additional_columns",
                column: "nomenclature_id",
                principalTable: "codeable_concepts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
