using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class CodeableConceptRegisterCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_codeable_concept_registers_codeable_concepts_codeable_conce",
                table: "codeable_concept_registers");

            migrationBuilder.DropIndex(
                name: "ix_codeable_concept_registers_codeable_concept_id_register_id",
                table: "codeable_concept_registers");

            migrationBuilder.DropColumn(
                name: "codeable_concept_id",
                table: "codeable_concept_registers");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "codeable_concept_registers",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "Код на номенклатура");

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "codeable_concept_registers",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "Тип на номенклатура");

            migrationBuilder.CreateIndex(
                name: "ix_codeable_concept_registers_type_code_register_id",
                table: "codeable_concept_registers",
                columns: new[] { "type", "code", "register_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_codeable_concept_registers_type_code_register_id",
                table: "codeable_concept_registers");

            migrationBuilder.DropColumn(
                name: "code",
                table: "codeable_concept_registers");

            migrationBuilder.DropColumn(
                name: "type",
                table: "codeable_concept_registers");

            migrationBuilder.AddColumn<long>(
                name: "codeable_concept_id",
                table: "codeable_concept_registers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Идентификатор на номенклатура");

            migrationBuilder.CreateIndex(
                name: "ix_codeable_concept_registers_codeable_concept_id_register_id",
                table: "codeable_concept_registers",
                columns: new[] { "codeable_concept_id", "register_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_codeable_concept_registers_codeable_concepts_codeable_conce",
                table: "codeable_concept_registers",
                column: "codeable_concept_id",
                principalTable: "codeable_concepts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
