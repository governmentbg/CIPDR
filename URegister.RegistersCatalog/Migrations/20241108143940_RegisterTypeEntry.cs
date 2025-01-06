using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RegisterTypeEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "identity_security_level",
                table: "registers",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                comment: "Ниво на осигуреност на средствата за електронна идентификация");

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "registers",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                comment: "Вид на регистъра");

            migrationBuilder.AddColumn<string>(
                name: "type_entry",
                table: "registers",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                comment: "Начин на вписване");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "identity_security_level",
                table: "registers");

            migrationBuilder.DropColumn(
                name: "type",
                table: "registers");

            migrationBuilder.DropColumn(
                name: "type_entry",
                table: "registers");
        }
    }
}
