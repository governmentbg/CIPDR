using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class RegisterItemProcessStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "process_step_id",
                table: "register_items",
                type: "uuid",
                nullable: true,
                comment: "Идентификатор на стъпка от процес");

            migrationBuilder.CreateIndex(
                name: "ix_register_items_process_step_id",
                table: "register_items",
                column: "process_step_id");

            migrationBuilder.AddForeignKey(
                name: "fk_register_items_process_steps_process_step_id",
                table: "register_items",
                column: "process_step_id",
                principalTable: "process_steps",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_register_items_process_steps_process_step_id",
                table: "register_items");

            migrationBuilder.DropIndex(
                name: "ix_register_items_process_step_id",
                table: "register_items");

            migrationBuilder.DropColumn(
                name: "process_step_id",
                table: "register_items");
        }
    }
}
