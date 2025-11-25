using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class stringLimitationsAddedToTableDeadlineDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "field_name",
                table: "public_field_templates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                comment: "Име на поле в Json",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldComment: "Име напле в Json");

            migrationBuilder.AlterColumn<string>(
                name: "deadline_type_id",
                table: "deadline_days",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                comment: "Вид срок за изпълнение на услуга",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Вид срок за изпълнение на услуга");

            migrationBuilder.AlterColumn<string>(
                name: "day_type_id",
                table: "deadline_days",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                comment: "Работни/календарни дни",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Работни/календарни дни");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "field_name",
                table: "public_field_templates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                comment: "Име напле в Json",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldComment: "Име на поле в Json");

            migrationBuilder.AlterColumn<string>(
                name: "deadline_type_id",
                table: "deadline_days",
                type: "text",
                nullable: false,
                comment: "Вид срок за изпълнение на услуга",
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11,
                oldComment: "Вид срок за изпълнение на услуга");

            migrationBuilder.AlterColumn<string>(
                name: "day_type_id",
                table: "deadline_days",
                type: "text",
                nullable: false,
                comment: "Работни/календарни дни",
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11,
                oldComment: "Работни/календарни дни");
        }
    }
}
