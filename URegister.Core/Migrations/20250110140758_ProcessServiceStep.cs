using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessServiceStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "last_service_step_id",
                table: "processes",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на стъпка");

            migrationBuilder.CreateIndex(
                name: "ix_processes_last_service_step_id",
                table: "processes",
                column: "last_service_step_id");

            migrationBuilder.AddForeignKey(
                name: "fk_processes_service_steps_last_service_step_id",
                table: "processes",
                column: "last_service_step_id",
                principalTable: "service_steps",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_processes_service_steps_last_service_step_id",
                table: "processes");

            migrationBuilder.DropIndex(
                name: "ix_processes_last_service_step_id",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "last_service_step_id",
                table: "processes");
        }
    }
}
