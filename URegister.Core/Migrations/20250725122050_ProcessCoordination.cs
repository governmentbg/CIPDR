using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessCoordination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "coordination_motive",
                table: "process_steps",
                type: "text",
                nullable: true,
                comment: "Мотиви при съгласуване");

            migrationBuilder.AddColumn<int>(
                name: "coordination_status_id",
                table: "process_steps",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Статус при съгласуване");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "coordination_motive",
                table: "process_steps");

            migrationBuilder.DropColumn(
                name: "coordination_status_id",
                table: "process_steps");
        }
    }
}
