using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RegisterAdministrationResourceMetaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "frequency_id",
                table: "register_administrations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Автоматично изпращане на данни към OpenData 1 ежедневно 2 седмично 3 месечно");

            migrationBuilder.AddColumn<string>(
                name: "resource_meta_id",
                table: "register_administrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "OpenData ResourceMetaId");

            migrationBuilder.AddColumn<int>(
                name: "frequency_id",
                table: "administration",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Автоматично изпращане на данни към OpenData 1 ежедневно 2 седмично 3 месечно");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "frequency_id",
                table: "register_administrations");

            migrationBuilder.DropColumn(
                name: "resource_meta_id",
                table: "register_administrations");

            migrationBuilder.DropColumn(
                name: "frequency_id",
                table: "administration");
        }
    }
}
