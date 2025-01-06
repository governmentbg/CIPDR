using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class OwnedPropertyRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "field_data",
                table: "field",
                type: "jsonb",
                nullable: false,
                comment: "Информация за полето. Настройки по подразбиране",
                oldClrType: typeof(string),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "field_data",
                table: "field",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldComment: "Информация за полето. Настройки по подразбиране");
        }
    }
}
