using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class NomenclatureType6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "nomenclature_types",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                comment: "Тип",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldComment: "Тип");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "codeable_concepts",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                comment: "Тип на номенклатура",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldComment: "Тип на номенклатура");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "nomenclature_types",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                comment: "Тип",
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6,
                oldComment: "Тип");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "codeable_concepts",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                comment: "Тип на номенклатура",
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6,
                oldComment: "Тип на номенклатура");
        }
    }
}
