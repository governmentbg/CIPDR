using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class FieldTypeExtended : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_complex_field",
                table: "field_type",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Дали полето е комплексно");

            migrationBuilder.AddColumn<string>(
                name: "template",
                table: "field_type",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                comment: "Шаблон за визуализация");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_complex_field",
                table: "field_type");

            migrationBuilder.DropColumn(
                name: "template",
                table: "field_type");
        }
    }
}
