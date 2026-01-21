using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class EFormServiceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "service_type_id",
                table: "register_services",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на услуга");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "service_type_id",
                table: "register_services");
        }
    }
}
