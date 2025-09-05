using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessFromId2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_processes_from_process_id",
                table: "processes",
                column: "from_process_id");

            migrationBuilder.AddForeignKey(
                name: "fk_processes_processes_from_process_id",
                table: "processes",
                column: "from_process_id",
                principalTable: "processes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_processes_processes_from_process_id",
                table: "processes");

            migrationBuilder.DropIndex(
                name: "ix_processes_from_process_id",
                table: "processes");
        }
    }
}
