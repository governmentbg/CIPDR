using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class OldIncimingNumberOnProcessMadeLonger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "old_incoming_number",
                table: "processes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Стар входящ номер",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComment: "Стар входящ номер");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "old_incoming_number",
                table: "processes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "Стар входящ номер",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Стар входящ номер");
        }
    }
}
