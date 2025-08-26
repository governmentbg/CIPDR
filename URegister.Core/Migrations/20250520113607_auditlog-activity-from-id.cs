using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class auditlogactivityfromid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "activity_from_id",
                table: "audits",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "Идентификатор на операция");

            migrationBuilder.AddColumn<int>(
                name: "register_id",
                table: "audits",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "activity_from_id",
                table: "audits");

            migrationBuilder.DropColumn(
                name: "register_id",
                table: "audits");
        }
    }
}
