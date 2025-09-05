using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessFormId2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_processes_form_id",
                table: "processes",
                column: "form_id");

            migrationBuilder.AddForeignKey(
                name: "fk_processes_forms_form_id",
                table: "processes",
                column: "form_id",
                principalTable: "forms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_processes_forms_form_id",
                table: "processes");

            migrationBuilder.DropIndex(
                name: "ix_processes_form_id",
                table: "processes");
        }
    }
}
