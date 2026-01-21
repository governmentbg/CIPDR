using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessStepTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "user_time_zone_offset_in_minutes",
                table: "process_steps",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Минути отстъп на потребителстата времева зона от UTC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_time_zone_offset_in_minutes",
                table: "process_steps");
        }
    }
}
