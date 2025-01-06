using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class StepTypeAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_for_official_use",
                table: "steps",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Стъпката е достъпна при официална услуга");

            migrationBuilder.AddColumn<bool>(
                name: "is_for_public_use",
                table: "steps",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Стъпката е достъпна при публична услуга");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_for_official_use",
                table: "steps");

            migrationBuilder.DropColumn(
                name: "is_for_public_use",
                table: "steps");
        }
    }
}
