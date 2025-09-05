using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class RegisterNumberMaxLengthAddedInTableProceess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "register_number",
                table: "processes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "Номер на вписване ",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Номер на вписване ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "register_number",
                table: "processes",
                type: "text",
                nullable: true,
                comment: "Номер на вписване ",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComment: "Номер на вписване ");
        }
    }
}
