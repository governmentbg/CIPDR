using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AdministrationPersonType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "uic",
                table: "administrations",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "",
                comment: "ЕИК");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "administration_person",
                type: "text",
                nullable: false,
                comment: "Тип лице",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Тип лице");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "uic",
                table: "administrations");

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "administration_person",
                type: "integer",
                nullable: false,
                comment: "Тип лице",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Тип лице");
        }
    }
}
