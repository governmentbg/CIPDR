using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.NomenclaturesCatalog.Migrations
{
    /// <inheritdoc />
    public partial class valueENDescriptionInCodeableConceptsFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confirmed",
                table: "codeable_concepts");

            migrationBuilder.AlterColumn<string>(
                name: "value_en",
                table: "codeable_concepts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                comment: "Стойност EN",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true,
                oldComment: "Стойност ЕН");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "codeable_concepts",
                type: "integer",
                nullable: false,
                comment: "Статус",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Одобрение");

            migrationBuilder.AlterColumn<string>(
                name: "value_en",
                table: "additional_columns",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                comment: "Стойност EN",
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true,
                oldComment: "Стойност ЕН");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "value_en",
                table: "codeable_concepts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                comment: "Стойност ЕН",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true,
                oldComment: "Стойност EN");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "codeable_concepts",
                type: "integer",
                nullable: false,
                comment: "Одобрение",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Статус");

            migrationBuilder.AddColumn<bool>(
                name: "confirmed",
                table: "codeable_concepts",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Одобрение");

            migrationBuilder.AlterColumn<string>(
                name: "value_en",
                table: "additional_columns",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                comment: "Стойност ЕН",
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true,
                oldComment: "Стойност EN");
        }
    }
}
