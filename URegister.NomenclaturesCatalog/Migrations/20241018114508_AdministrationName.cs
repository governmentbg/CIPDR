using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AdministrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_codeable_concept_registers_codeable_concepts_nomenclature_id",
                table: "codeable_concept_registers");

            migrationBuilder.DropForeignKey(
                name: "fk_nomenclature_type_registers_nomenclature_types_nomenclature",
                table: "nomenclature_type_registers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_nomenclature_type_registers",
                table: "nomenclature_type_registers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_codeable_concept_registers",
                table: "codeable_concept_registers");

            migrationBuilder.RenameTable(
                name: "nomenclature_type_registers",
                newName: "nomenclature_type_administrations");

            migrationBuilder.RenameTable(
                name: "codeable_concept_registers",
                newName: "codeable_concept_administrations");

            migrationBuilder.RenameIndex(
                name: "ix_nomenclature_type_registers_nomenclature_type_id_administra",
                table: "nomenclature_type_administrations",
                newName: "ix_nomenclature_type_administrations_nomenclature_type_id_admi");

            migrationBuilder.RenameColumn(
                name: "nomenclature_id",
                table: "codeable_concept_administrations",
                newName: "codeable_concept_id");

            migrationBuilder.RenameIndex(
                name: "ix_codeable_concept_registers_nomenclature_id_administration_id",
                table: "codeable_concept_administrations",
                newName: "ix_codeable_concept_administrations_codeable_concept_id_admini");

            migrationBuilder.AlterTable(
                name: "codeable_concept_administrations",
                comment: "Допустимост на номенклатура за регистъра",
                oldComment: "Номенклатура");

            migrationBuilder.AddPrimaryKey(
                name: "pk_nomenclature_type_administrations",
                table: "nomenclature_type_administrations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_codeable_concept_administrations",
                table: "codeable_concept_administrations",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_codeable_concept_administrations_codeable_concepts_codeable",
                table: "codeable_concept_administrations",
                column: "codeable_concept_id",
                principalTable: "codeable_concepts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_nomenclature_type_administrations_nomenclature_types_nomenc",
                table: "nomenclature_type_administrations",
                column: "nomenclature_type_id",
                principalTable: "nomenclature_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_codeable_concept_administrations_codeable_concepts_codeable",
                table: "codeable_concept_administrations");

            migrationBuilder.DropForeignKey(
                name: "fk_nomenclature_type_administrations_nomenclature_types_nomenc",
                table: "nomenclature_type_administrations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_nomenclature_type_administrations",
                table: "nomenclature_type_administrations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_codeable_concept_administrations",
                table: "codeable_concept_administrations");

            migrationBuilder.RenameTable(
                name: "nomenclature_type_administrations",
                newName: "nomenclature_type_registers");

            migrationBuilder.RenameTable(
                name: "codeable_concept_administrations",
                newName: "codeable_concept_registers");

            migrationBuilder.RenameIndex(
                name: "ix_nomenclature_type_administrations_nomenclature_type_id_admi",
                table: "nomenclature_type_registers",
                newName: "ix_nomenclature_type_registers_nomenclature_type_id_administra");

            migrationBuilder.RenameColumn(
                name: "codeable_concept_id",
                table: "codeable_concept_registers",
                newName: "nomenclature_id");

            migrationBuilder.RenameIndex(
                name: "ix_codeable_concept_administrations_codeable_concept_id_admini",
                table: "codeable_concept_registers",
                newName: "ix_codeable_concept_registers_nomenclature_id_administration_id");

            migrationBuilder.AlterTable(
                name: "codeable_concept_registers",
                comment: "Номенклатура",
                oldComment: "Допустимост на номенклатура за регистъра");

            migrationBuilder.AddPrimaryKey(
                name: "pk_nomenclature_type_registers",
                table: "nomenclature_type_registers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_codeable_concept_registers",
                table: "codeable_concept_registers",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_codeable_concept_registers_codeable_concepts_nomenclature_id",
                table: "codeable_concept_registers",
                column: "nomenclature_id",
                principalTable: "codeable_concepts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_nomenclature_type_registers_nomenclature_types_nomenclature",
                table: "nomenclature_type_registers",
                column: "nomenclature_type_id",
                principalTable: "nomenclature_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
