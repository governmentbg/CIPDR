using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.AuditLog.Migrations
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "activity_from_id",
                table: "audits");
        }
    }
}
