using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessFormId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_processes_administrations_tennant_id",
                table: "processes");

            migrationBuilder.RenameColumn(
                name: "tennant_id",
                table: "processes",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "ix_processes_tennant_id",
                table: "processes",
                newName: "ix_processes_tenant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_processes_administrations_tenant_id",
                table: "processes",
                column: "tenant_id",
                principalTable: "administrations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_processes_administrations_tenant_id",
                table: "processes");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "processes",
                newName: "tennant_id");

            migrationBuilder.RenameIndex(
                name: "ix_processes_tenant_id",
                table: "processes",
                newName: "ix_processes_tennant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_processes_administrations_tennant_id",
                table: "processes",
                column: "tennant_id",
                principalTable: "administrations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
