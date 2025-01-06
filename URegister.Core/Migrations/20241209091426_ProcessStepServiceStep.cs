using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessStepServiceStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_process_steps_service_steps_step_id",
                table: "process_steps");

            migrationBuilder.RenameColumn(
                name: "step_id",
                table: "process_steps",
                newName: "service_step_id");

            migrationBuilder.RenameIndex(
                name: "ix_process_steps_step_id",
                table: "process_steps",
                newName: "ix_process_steps_service_step_id");

            migrationBuilder.AddForeignKey(
                name: "fk_process_steps_service_steps_service_step_id",
                table: "process_steps",
                column: "service_step_id",
                principalTable: "service_steps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_process_steps_service_steps_service_step_id",
                table: "process_steps");

            migrationBuilder.RenameColumn(
                name: "service_step_id",
                table: "process_steps",
                newName: "step_id");

            migrationBuilder.RenameIndex(
                name: "ix_process_steps_service_step_id",
                table: "process_steps",
                newName: "ix_process_steps_step_id");

            migrationBuilder.AddForeignKey(
                name: "fk_process_steps_service_steps_step_id",
                table: "process_steps",
                column: "step_id",
                principalTable: "service_steps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
