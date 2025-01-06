using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class LabelMadeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "label",
                table: "field_type",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                comment: "Етикет на поле",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Етикет на поле");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "label",
                table: "field_type",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Етикет на поле",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldComment: "Етикет на поле");
        }
    }
}
