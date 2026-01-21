using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class formParentIdAddedToFieldFormula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "form_parent_id",
                table: "field_formulas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на първата версия на формата");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "form_parent_id",
                table: "field_formulas");
        }
    }
}
