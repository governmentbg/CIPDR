using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class NomenclatureRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "codeable_concept_administrations");

            migrationBuilder.DropTable(
                name: "nomenclature_type_administrations");

            migrationBuilder.CreateTable(
                name: "codeable_concept_registers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codeable_concept_id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор на номенклатура"),
                    register_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на регистър"),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false, comment: "Допустима ли е за регистъра"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Създаден от"),
                    created_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата и час на записа")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_codeable_concept_registers", x => x.id);
                    table.ForeignKey(
                        name: "fk_codeable_concept_registers_codeable_concepts_codeable_conce",
                        column: x => x.codeable_concept_id,
                        principalTable: "codeable_concepts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Допустимост на номенклатура за регистъра");

            migrationBuilder.CreateTable(
                name: "nomenclature_type_registers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nomenclature_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на тип"),
                    register_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на регистър"),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false, comment: "Допустима ли е за регистъра"),
                    is_valid_all_items = table.Column<bool>(type: "boolean", nullable: false, comment: "Допустими ли са всички стойности от CodeableConcept"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Създаден от"),
                    created_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата и час на записа")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nomenclature_type_registers", x => x.id);
                    table.ForeignKey(
                        name: "fk_nomenclature_type_registers_nomenclature_types_nomenclature",
                        column: x => x.nomenclature_type_id,
                        principalTable: "nomenclature_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Допустим тип номенклатура");

            migrationBuilder.CreateIndex(
                name: "ix_codeable_concepts_type_code_date_from",
                table: "codeable_concepts",
                columns: new[] { "type", "code", "date_from" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_codeable_concept_registers_codeable_concept_id_register_id",
                table: "codeable_concept_registers",
                columns: new[] { "codeable_concept_id", "register_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_nomenclature_type_registers_nomenclature_type_id_register_id",
                table: "nomenclature_type_registers",
                columns: new[] { "nomenclature_type_id", "register_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "codeable_concept_registers");

            migrationBuilder.DropTable(
                name: "nomenclature_type_registers");

            migrationBuilder.DropIndex(
                name: "ix_codeable_concepts_type_code_date_from",
                table: "codeable_concepts");

            migrationBuilder.CreateTable(
                name: "codeable_concept_administrations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codeable_concept_id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор на номенклатура"),
                    administration_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на администрация"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Създаден от"),
                    created_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата и час на записа"),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false, comment: "Допустима ли е за регистъра")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_codeable_concept_administrations", x => x.id);
                    table.ForeignKey(
                        name: "fk_codeable_concept_administrations_codeable_concepts_codeable",
                        column: x => x.codeable_concept_id,
                        principalTable: "codeable_concepts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Допустимост на номенклатура за регистъра");

            migrationBuilder.CreateTable(
                name: "nomenclature_type_administrations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nomenclature_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на тип"),
                    administration_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на администрация"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Създаден от"),
                    created_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата и час на записа"),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false, comment: "Допустима ли е за регистъра"),
                    is_valid_all_items = table.Column<bool>(type: "boolean", nullable: false, comment: "Допустими ли са всички стойности от CodeableConcept")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nomenclature_type_administrations", x => x.id);
                    table.ForeignKey(
                        name: "fk_nomenclature_type_administrations_nomenclature_types_nomenc",
                        column: x => x.nomenclature_type_id,
                        principalTable: "nomenclature_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Допустим тип номенклатура");

            migrationBuilder.CreateIndex(
                name: "ix_codeable_concept_administrations_codeable_concept_id_admini",
                table: "codeable_concept_administrations",
                columns: new[] { "codeable_concept_id", "administration_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_nomenclature_type_administrations_nomenclature_type_id_admi",
                table: "nomenclature_type_administrations",
                columns: new[] { "nomenclature_type_id", "administration_id" },
                unique: true);
        }
    }
}
