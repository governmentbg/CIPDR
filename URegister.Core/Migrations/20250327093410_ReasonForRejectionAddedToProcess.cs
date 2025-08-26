using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ReasonForRejectionAddedToProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reason_for_rejection",
                table: "processes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Причина за прекратяване");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reason_for_rejection",
                table: "processes");
        }
    }
}
