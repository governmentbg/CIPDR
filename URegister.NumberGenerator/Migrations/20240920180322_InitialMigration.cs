using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.NumberGenerator.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "number_archives",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    register = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Код на регистър"),
                    number = table.Column<long>(type: "bigint", nullable: false, comment: "Номер"),
                    initial_document_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на инициращият документ")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_number_archives", x => x.id);
                },
                comment: "Архив на номератора");

            migrationBuilder.CreateTable(
                name: "numerators",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    administration_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Код на администрацията"),
                    year = table.Column<int>(type: "integer", nullable: false, comment: "Година"),
                    sequence = table.Column<int>(type: "integer", nullable: false, comment: "Последователност")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_numerators", x => x.id);
                },
                comment: "Номератори на институциите използващи универсалния регистър");

            migrationBuilder.CreateIndex(
                name: "ix_number_archives_number",
                table: "number_archives",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_numerators_administration_code_year",
                table: "numerators",
                columns: new[] { "administration_code", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "number_archives");

            migrationBuilder.DropTable(
                name: "numerators");
        }
    }
}
