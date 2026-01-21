using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class NameEDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name_e_delivery",
                table: "registers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "Съкратено име на регистър ползва се при пращане на съобщение към ССЕВ");

            migrationBuilder.AddColumn<string>(
                name: "name_e_delivery",
                table: "administration",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "Съкратено име на администрация ползва се при пращане на съобщение към ССЕВ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name_e_delivery",
                table: "registers");

            migrationBuilder.DropColumn(
                name: "name_e_delivery",
                table: "administration");
        }
    }
}
