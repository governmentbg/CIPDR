using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ekatte");

            migrationBuilder.CreateTable(
                name: "codeable_concepts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Код"),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Стойност"),
                    value_en = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Стойност ЕН"),
                    date_from = table.Column<DateTime>(type: "date", nullable: false, comment: "Валидна от дата"),
                    date_to = table.Column<DateTime>(type: "date", nullable: true, comment: "Валидна до дата"),
                    type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, comment: "Тип на номенклатура"),
                    parent_code = table.Column<string>(type: "text", nullable: true, comment: "Код не горно ниво при дървовидна номенклатура"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Създаден от"),
                    created_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата и час на записа")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_codeable_concepts", x => x.id);
                },
                comment: "Номенклатура");

            migrationBuilder.CreateTable(
                name: "ek_areas",
                schema: "ekatte",
                columns: table => new
                {
                    aread_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    region = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    document = table.Column<string>(type: "text", nullable: true),
                    abc = table.Column<string>(type: "text", nullable: true),
                    name_en = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ek_areas", x => x.aread_id);
                });

            migrationBuilder.CreateTable(
                name: "ek_countries",
                schema: "ekatte",
                columns: table => new
                {
                    country_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ek_countries", x => x.country_id);
                });

            migrationBuilder.CreateTable(
                name: "ek_streets",
                schema: "ekatte",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    ekatte = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    date_from = table.Column<DateTime>(type: "date", nullable: false),
                    date_to = table.Column<DateTime>(type: "date", nullable: true),
                    street_type = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ek_streets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nomenclature_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, comment: "Тип"),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Име")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nomenclature_types", x => x.id);
                },
                comment: "Тип номенклатура");

            migrationBuilder.CreateTable(
                name: "workflows",
                columns: table => new
                {
                    workflow_name = table.Column<string>(type: "text", nullable: false),
                    rule_expression_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflows", x => x.workflow_name);
                });

            migrationBuilder.CreateTable(
                name: "additional_columns",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nomenclature_id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор на номенклатура"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Име на колона"),
                    value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false, comment: "Стойност"),
                    value_en = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true, comment: "Стойност ЕН")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_additional_columns", x => x.id);
                    table.ForeignKey(
                        name: "fk_additional_columns_codeable_concepts_nomenclature_id",
                        column: x => x.nomenclature_id,
                        principalTable: "codeable_concepts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Допълнителни данни за номенклатура");

            migrationBuilder.CreateTable(
                name: "codeable_concept_registers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nomenclature_id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор на номенклатура"),
                    administration_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на администрация"),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false, comment: "Допустима ли е за регистъра"),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "Създаден от"),
                    created_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата и час на записа")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_codeable_concept_registers", x => x.id);
                    table.ForeignKey(
                        name: "fk_codeable_concept_registers_codeable_concepts_nomenclature_id",
                        column: x => x.nomenclature_id,
                        principalTable: "codeable_concepts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Номенклатура");

            migrationBuilder.CreateTable(
                name: "ek_districts",
                schema: "ekatte",
                columns: table => new
                {
                    district_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    oblast = table.Column<string>(type: "text", nullable: false),
                    ekatte = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    region = table.Column<string>(type: "text", nullable: false),
                    document = table.Column<string>(type: "text", nullable: true),
                    abc = table.Column<string>(type: "text", nullable: true),
                    rghi = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    country_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ek_districts", x => x.district_id);
                    table.ForeignKey(
                        name: "fk_ek_districts_ek_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "ekatte",
                        principalTable: "ek_countries",
                        principalColumn: "country_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nomenclature_type_registers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nomenclature_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на тип"),
                    administration_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на администрация"),
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

            migrationBuilder.CreateTable(
                name: "rules",
                columns: table => new
                {
                    rule_name = table.Column<string>(type: "text", nullable: false),
                    properties = table.Column<string>(type: "text", nullable: true),
                    @operator = table.Column<string>(name: "operator", type: "text", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    rule_expression_type = table.Column<int>(type: "integer", nullable: false),
                    expression = table.Column<string>(type: "text", nullable: true),
                    actions = table.Column<string>(type: "text", nullable: true),
                    success_event = table.Column<string>(type: "text", nullable: true),
                    rule_name_fk = table.Column<string>(type: "text", nullable: true),
                    workflow_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rules", x => x.rule_name);
                    table.ForeignKey(
                        name: "fk_rules_rules_rule_name_fk",
                        column: x => x.rule_name_fk,
                        principalTable: "rules",
                        principalColumn: "rule_name");
                    table.ForeignKey(
                        name: "fk_rules_workflows_workflow_name",
                        column: x => x.workflow_name,
                        principalTable: "workflows",
                        principalColumn: "workflow_name");
                });

            migrationBuilder.CreateTable(
                name: "ek_munincipalities",
                schema: "ekatte",
                columns: table => new
                {
                    municipality_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    municipality = table.Column<string>(type: "text", nullable: false),
                    ekatte = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true),
                    document = table.Column<string>(type: "text", nullable: true),
                    abc = table.Column<string>(type: "text", nullable: true),
                    district_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ek_munincipalities", x => x.municipality_id);
                    table.ForeignKey(
                        name: "fk_ek_munincipalities_ek_districts_district_id",
                        column: x => x.district_id,
                        principalSchema: "ekatte",
                        principalTable: "ek_districts",
                        principalColumn: "district_id");
                });

            migrationBuilder.CreateTable(
                name: "scoped_param",
                columns: table => new
                {
                    name = table.Column<string>(type: "text", nullable: false),
                    expression = table.Column<string>(type: "text", nullable: true),
                    rule_name = table.Column<string>(type: "text", nullable: true),
                    workflow_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scoped_param", x => x.name);
                    table.ForeignKey(
                        name: "fk_scoped_param_rules_rule_name",
                        column: x => x.rule_name,
                        principalTable: "rules",
                        principalColumn: "rule_name");
                    table.ForeignKey(
                        name: "fk_scoped_param_workflows_workflow_name",
                        column: x => x.workflow_name,
                        principalTable: "workflows",
                        principalColumn: "workflow_name");
                });

            migrationBuilder.CreateTable(
                name: "ek_ekatte",
                schema: "ekatte",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ekatte = table.Column<string>(type: "text", nullable: false),
                    t_v_m = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    oblast = table.Column<string>(type: "text", nullable: false),
                    obstina = table.Column<string>(type: "text", nullable: false),
                    kmetstvo = table.Column<string>(type: "text", nullable: true),
                    kind = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true),
                    altitude = table.Column<string>(type: "text", nullable: true),
                    document = table.Column<string>(type: "text", nullable: true),
                    tsb = table.Column<string>(type: "text", nullable: true),
                    abc = table.Column<string>(type: "text", nullable: true),
                    district_id = table.Column<int>(type: "integer", nullable: true),
                    country_id = table.Column<int>(type: "integer", nullable: false),
                    municipal_id = table.Column<int>(type: "integer", nullable: true),
                    munincipality_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ek_ekatte", x => x.id);
                    table.ForeignKey(
                        name: "fk_ek_ekatte_ek_countries_country_id",
                        column: x => x.country_id,
                        principalSchema: "ekatte",
                        principalTable: "ek_countries",
                        principalColumn: "country_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ek_ekatte_ek_districts_district_id",
                        column: x => x.district_id,
                        principalSchema: "ekatte",
                        principalTable: "ek_districts",
                        principalColumn: "district_id");
                    table.ForeignKey(
                        name: "fk_ek_ekatte_ek_munincipalities_municipal_id",
                        column: x => x.municipal_id,
                        principalSchema: "ekatte",
                        principalTable: "ek_munincipalities",
                        principalColumn: "municipality_id");
                });

            migrationBuilder.CreateTable(
                name: "ek_sobr",
                schema: "ekatte",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ekatte = table.Column<string>(type: "text", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    area1 = table.Column<string>(type: "text", nullable: true),
                    area2 = table.Column<string>(type: "text", nullable: true),
                    document = table.Column<string>(type: "text", nullable: true),
                    abc = table.Column<string>(type: "text", nullable: true),
                    district_id = table.Column<int>(type: "integer", nullable: true),
                    municipal_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ek_sobr", x => x.id);
                    table.ForeignKey(
                        name: "fk_ek_sobr_ek_districts_district_id",
                        column: x => x.district_id,
                        principalSchema: "ekatte",
                        principalTable: "ek_districts",
                        principalColumn: "district_id");
                    table.ForeignKey(
                        name: "fk_ek_sobr_ek_munincipalities_municipal_id",
                        column: x => x.municipal_id,
                        principalSchema: "ekatte",
                        principalTable: "ek_munincipalities",
                        principalColumn: "municipality_id");
                });

            migrationBuilder.CreateTable(
                name: "ek_regions",
                schema: "ekatte",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    raion = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true),
                    document = table.Column<string>(type: "text", nullable: true),
                    settlement_id = table.Column<int>(type: "integer", nullable: true),
                    region_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ek_regions", x => x.id);
                    table.ForeignKey(
                        name: "fk_ek_regions_ek_ekatte_settlement_id",
                        column: x => x.settlement_id,
                        principalSchema: "ekatte",
                        principalTable: "ek_ekatte",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_additional_columns_nomenclature_id",
                table: "additional_columns",
                column: "nomenclature_id");

            migrationBuilder.CreateIndex(
                name: "ix_codeable_concept_registers_nomenclature_id",
                table: "codeable_concept_registers",
                column: "nomenclature_id");

            migrationBuilder.CreateIndex(
                name: "ix_ek_districts_country_id",
                schema: "ekatte",
                table: "ek_districts",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "ix_ek_ekatte_country_id",
                schema: "ekatte",
                table: "ek_ekatte",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "ix_ek_ekatte_district_id",
                schema: "ekatte",
                table: "ek_ekatte",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "ix_ek_ekatte_municipal_id",
                schema: "ekatte",
                table: "ek_ekatte",
                column: "municipal_id");

            migrationBuilder.CreateIndex(
                name: "ix_ek_munincipalities_district_id",
                schema: "ekatte",
                table: "ek_munincipalities",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "ix_ek_regions_settlement_id",
                schema: "ekatte",
                table: "ek_regions",
                column: "settlement_id");

            migrationBuilder.CreateIndex(
                name: "ix_ek_sobr_district_id",
                schema: "ekatte",
                table: "ek_sobr",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "ix_ek_sobr_municipal_id",
                schema: "ekatte",
                table: "ek_sobr",
                column: "municipal_id");

            migrationBuilder.CreateIndex(
                name: "ix_nomenclature_type_registers_nomenclature_type_id",
                table: "nomenclature_type_registers",
                column: "nomenclature_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_rules_rule_name_fk",
                table: "rules",
                column: "rule_name_fk");

            migrationBuilder.CreateIndex(
                name: "ix_rules_workflow_name",
                table: "rules",
                column: "workflow_name");

            migrationBuilder.CreateIndex(
                name: "ix_scoped_param_rule_name",
                table: "scoped_param",
                column: "rule_name");

            migrationBuilder.CreateIndex(
                name: "ix_scoped_param_workflow_name",
                table: "scoped_param",
                column: "workflow_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "additional_columns");

            migrationBuilder.DropTable(
                name: "codeable_concept_registers");

            migrationBuilder.DropTable(
                name: "ek_areas",
                schema: "ekatte");

            migrationBuilder.DropTable(
                name: "ek_regions",
                schema: "ekatte");

            migrationBuilder.DropTable(
                name: "ek_sobr",
                schema: "ekatte");

            migrationBuilder.DropTable(
                name: "ek_streets",
                schema: "ekatte");

            migrationBuilder.DropTable(
                name: "nomenclature_type_registers");

            migrationBuilder.DropTable(
                name: "scoped_param");

            migrationBuilder.DropTable(
                name: "codeable_concepts");

            migrationBuilder.DropTable(
                name: "ek_ekatte",
                schema: "ekatte");

            migrationBuilder.DropTable(
                name: "nomenclature_types");

            migrationBuilder.DropTable(
                name: "rules");

            migrationBuilder.DropTable(
                name: "ek_munincipalities",
                schema: "ekatte");

            migrationBuilder.DropTable(
                name: "workflows");

            migrationBuilder.DropTable(
                name: "ek_districts",
                schema: "ekatte");

            migrationBuilder.DropTable(
                name: "ek_countries",
                schema: "ekatte");
        }
    }
}
