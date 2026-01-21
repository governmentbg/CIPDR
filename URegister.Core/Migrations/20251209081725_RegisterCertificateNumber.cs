using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class RegisterCertificateNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "register_certificate_number",
                table: "processes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "Номер на удостоверение при вписване");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "register_certificate_number",
                table: "processes");
        }
    }
}
