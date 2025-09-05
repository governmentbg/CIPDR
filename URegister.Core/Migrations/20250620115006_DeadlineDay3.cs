using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class DeadlineDay3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "deadline_type_id",
                table: "deadline_days",
                type: "text",
                nullable: false,
                comment: "Вид срок за изпълнение на услуга",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Вид срок за изпълнение на услуга");

            migrationBuilder.AlterColumn<string>(
                name: "day_type_id",
                table: "deadline_days",
                type: "text",
                nullable: false,
                comment: "Работни/календарни дни",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Работни/календарни дни");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "deadline_type_id",
                table: "deadline_days",
                type: "integer",
                nullable: false,
                comment: "Вид срок за изпълнение на услуга",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Вид срок за изпълнение на услуга");

            migrationBuilder.AlterColumn<int>(
                name: "day_type_id",
                table: "deadline_days",
                type: "integer",
                nullable: false,
                comment: "Работни/календарни дни",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Работни/календарни дни");
        }
    }
}
