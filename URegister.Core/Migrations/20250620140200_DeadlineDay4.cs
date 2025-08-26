using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class DeadlineDay4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "form_parent_id",
                table: "deadline_days",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "service_id",
                table: "deadline_days",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_deadline_days_service_id",
                table: "deadline_days",
                column: "service_id");

            migrationBuilder.AddForeignKey(
                name: "fk_deadline_days_services_service_id",
                table: "deadline_days",
                column: "service_id",
                principalTable: "services",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_deadline_days_services_service_id",
                table: "deadline_days");

            migrationBuilder.DropIndex(
                name: "ix_deadline_days_service_id",
                table: "deadline_days");

            migrationBuilder.DropColumn(
                name: "form_parent_id",
                table: "deadline_days");

            migrationBuilder.DropColumn(
                name: "service_id",
                table: "deadline_days");
        }
    }
}
