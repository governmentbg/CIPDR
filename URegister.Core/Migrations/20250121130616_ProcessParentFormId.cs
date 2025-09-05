using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessParentFormId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_process_steps_forms_form_id",
                table: "process_steps");

            migrationBuilder.DropIndex(
                name: "ix_process_steps_form_id",
                table: "process_steps");

            migrationBuilder.DropColumn(
                name: "form_parent_id",
                table: "service_steps");

            migrationBuilder.DropColumn(
                name: "form_id",
                table: "process_steps");

            migrationBuilder.AddColumn<int>(
                name: "form_parent_id",
                table: "services",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на тип форма");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "form_parent_id",
                table: "services");

            migrationBuilder.AddColumn<int>(
                name: "form_parent_id",
                table: "service_steps",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на тип форма");

            migrationBuilder.AddColumn<int>(
                name: "form_id",
                table: "process_steps",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на форма");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_form_id",
                table: "process_steps",
                column: "form_id");

            migrationBuilder.AddForeignKey(
                name: "fk_process_steps_forms_form_id",
                table: "process_steps",
                column: "form_id",
                principalTable: "forms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
