using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class englishNameAddedToRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name_en",
                table: "registers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                comment: "Име на регистър на английски език");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name_en",
                table: "registers");
        }
    }
}
