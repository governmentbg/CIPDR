using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class englishNamesAddedToRegisterAndAdministration : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "name_en",
                table: "administration",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                comment: "Име на английски език");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name_en",
                table: "registers");

            migrationBuilder.DropColumn(
                name: "name_en",
                table: "administration");
        }
    }
}
