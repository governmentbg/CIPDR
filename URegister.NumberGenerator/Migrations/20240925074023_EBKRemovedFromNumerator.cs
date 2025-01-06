using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NumberGenerator.Migrations
{
    /// <inheritdoc />
    public partial class EBKRemovedFromNumerator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_numerators_administration_code_year",
                table: "numerators");

            migrationBuilder.DropColumn(
                name: "administration_code",
                table: "numerators");

            migrationBuilder.DropColumn(
                name: "year",
                table: "numerators");

            migrationBuilder.AlterTable(
                name: "numerators",
                comment: "Номератор на универсалния регистър",
                oldComment: "Номератори на институциите използващи универсалния регистър");

            migrationBuilder.AddColumn<int>(
                name: "prefix",
                table: "numerators",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Префикс във формат yyddd");

            migrationBuilder.CreateIndex(
                name: "ix_numerators_prefix",
                table: "numerators",
                column: "prefix",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_numerators_prefix",
                table: "numerators");

            migrationBuilder.DropColumn(
                name: "prefix",
                table: "numerators");

            migrationBuilder.AlterTable(
                name: "numerators",
                comment: "Номератори на институциите използващи универсалния регистър",
                oldComment: "Номератор на универсалния регистър");

            migrationBuilder.AddColumn<string>(
                name: "administration_code",
                table: "numerators",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                comment: "Код на администрацията");

            migrationBuilder.AddColumn<int>(
                name: "year",
                table: "numerators",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Година");

            migrationBuilder.CreateIndex(
                name: "ix_numerators_administration_code_year",
                table: "numerators",
                columns: new[] { "administration_code", "year" },
                unique: true);
        }
    }
}
