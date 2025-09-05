using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class IsColumnInDataTableAddedInRegisterItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_column_in_data_table",
                table: "register_items",
                type: "boolean",
                nullable: true,
                comment: "Дали полето ще бъде колона при визуализиране на данните в таблица");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_column_in_data_table",
                table: "register_items");
        }
    }
}
