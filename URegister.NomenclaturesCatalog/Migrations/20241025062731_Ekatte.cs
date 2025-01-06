using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class Ekatte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "ek_ekatte",
                schema: "ekatte");

            migrationBuilder.DropTable(
                name: "ek_munincipalities",
                schema: "ekatte");

            migrationBuilder.DropTable(
                name: "ek_districts",
                schema: "ekatte");

            migrationBuilder.DropTable(
                name: "ek_countries",
                schema: "ekatte");

            migrationBuilder.AddColumn<string>(
                name: "holder_type",
                table: "nomenclature_types",
                type: "text",
                nullable: true,
                comment: "Горно ниво в друг номенклатурен тип");

            migrationBuilder.AddColumn<string>(
                name: "holder_code",
                table: "codeable_concepts",
                type: "text",
                nullable: true,
                comment: "Горно ниво в друг номенклатурен тип");

            migrationBuilder.CreateTable(
                name: "ek_doc",
                columns: table => new
                {
                    document = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    doc_kind = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    doc_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    doc_name_en = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    doc_inst = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    doc_num = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    doc_date = table.Column<DateTime>(type: "date", nullable: true),
                    doc_act = table.Column<DateTime>(type: "date", nullable: true),
                    dv_danni = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    dv_date = table.Column<DateTime>(type: "date", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ek_doc", x => x.document);
                },
                comment: "Импрортирани документи от nrnm.nsi.bg");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ek_doc");

            migrationBuilder.DropColumn(
                name: "holder_type",
                table: "nomenclature_types");

            migrationBuilder.DropColumn(
                name: "holder_code",
                table: "codeable_concepts");

            migrationBuilder.EnsureSchema(
                name: "ekatte");

            migrationBuilder.CreateTable(
                name: "ek_areas",
                schema: "ekatte",
                columns: table => new
                {
                    aread_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    abc = table.Column<string>(type: "text", nullable: true),
                    document = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    name_en = table.Column<string>(type: "text", nullable: true),
                    region = table.Column<string>(type: "text", nullable: false)
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
                    date_from = table.Column<DateTime>(type: "date", nullable: false),
                    date_to = table.Column<DateTime>(type: "date", nullable: true),
                    ekatte = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    street_type = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ek_streets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ek_districts",
                schema: "ekatte",
                columns: table => new
                {
                    district_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    country_id = table.Column<int>(type: "integer", nullable: false),
                    abc = table.Column<string>(type: "text", nullable: true),
                    document = table.Column<string>(type: "text", nullable: true),
                    ekatte = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    oblast = table.Column<string>(type: "text", nullable: false),
                    region = table.Column<string>(type: "text", nullable: false),
                    rghi = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true)
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
                name: "ek_munincipalities",
                schema: "ekatte",
                columns: table => new
                {
                    municipality_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    district_id = table.Column<int>(type: "integer", nullable: true),
                    abc = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "text", nullable: true),
                    document = table.Column<string>(type: "text", nullable: true),
                    ekatte = table.Column<string>(type: "text", nullable: false),
                    municipality = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
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
                name: "ek_ekatte",
                schema: "ekatte",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    country_id = table.Column<int>(type: "integer", nullable: false),
                    district_id = table.Column<int>(type: "integer", nullable: true),
                    municipal_id = table.Column<int>(type: "integer", nullable: true),
                    abc = table.Column<string>(type: "text", nullable: true),
                    altitude = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "text", nullable: true),
                    document = table.Column<string>(type: "text", nullable: true),
                    ekatte = table.Column<string>(type: "text", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    kmetstvo = table.Column<string>(type: "text", nullable: true),
                    munincipality_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    oblast = table.Column<string>(type: "text", nullable: false),
                    obstina = table.Column<string>(type: "text", nullable: false),
                    t_v_m = table.Column<string>(type: "text", nullable: true),
                    tsb = table.Column<string>(type: "text", nullable: true)
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
                    district_id = table.Column<int>(type: "integer", nullable: true),
                    municipal_id = table.Column<int>(type: "integer", nullable: true),
                    abc = table.Column<string>(type: "text", nullable: true),
                    area1 = table.Column<string>(type: "text", nullable: true),
                    area2 = table.Column<string>(type: "text", nullable: true),
                    document = table.Column<string>(type: "text", nullable: true),
                    ekatte = table.Column<string>(type: "text", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
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
                    settlement_id = table.Column<int>(type: "integer", nullable: true),
                    category = table.Column<string>(type: "text", nullable: true),
                    document = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    raion = table.Column<string>(type: "text", nullable: false),
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
        }
    }
}
