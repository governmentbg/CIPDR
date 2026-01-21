using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class stringLimitationsAddedToPublicFieldTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "label",
                table: "public_field_templates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                comment: "Наименование на публично поле",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Наименование на публично поле");

            migrationBuilder.AlterColumn<string>(
                name: "field_name",
                table: "public_field_templates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                comment: "Име напле в Json",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Име напле в Json");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "public_field_templates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                comment: "Създадена от",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Създадена от");

            migrationBuilder.AlterColumn<string>(
                name: "content",
                table: "public_field_templates",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Съдържание на бланка",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Съдържание на бланка");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "label",
                table: "public_field_templates",
                type: "text",
                nullable: false,
                comment: "Наименование на публично поле",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldComment: "Наименование на публично поле");

            migrationBuilder.AlterColumn<string>(
                name: "field_name",
                table: "public_field_templates",
                type: "text",
                nullable: false,
                comment: "Име напле в Json",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldComment: "Име напле в Json");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "public_field_templates",
                type: "text",
                nullable: true,
                comment: "Създадена от",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true,
                oldComment: "Създадена от");

            migrationBuilder.AlterColumn<string>(
                name: "content",
                table: "public_field_templates",
                type: "text",
                nullable: true,
                comment: "Съдържание на бланка",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "Съдържание на бланка");
        }
    }
}
