using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessStepOrderNum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "order_num",
                table: "process_steps",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Поредност на стъпка");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "order_num",
                table: "process_steps");
        }
    }
}
