using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class EFormRegisteredServiceNumberMovedFromProcessStepToProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "e_form_registered_service_number",
                table: "process_steps");

            migrationBuilder.AddColumn<Guid>(
                name: "e_form_registered_service_number",
                table: "processes",
                type: "uuid",
                nullable: true,
                comment: "Номер на заявена услуга при импорт от е-форма");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "e_form_registered_service_number",
                table: "processes");

            migrationBuilder.AddColumn<Guid>(
                name: "e_form_registered_service_number",
                table: "process_steps",
                type: "uuid",
                nullable: true,
                comment: "Номер на заявена услуга при импорт от е-форма");
        }
    }
}
