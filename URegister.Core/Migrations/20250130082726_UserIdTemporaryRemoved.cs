using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class UserIdTemporaryRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "modified_by_user_id",
                table: "forms");

            migrationBuilder.DropColumn(
                name: "modified_by_user_id",
                table: "file_metadata");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "modified_by_user_id",
                table: "forms",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "",
                comment: "Идентификатор на потребилет променил последно записа");

            migrationBuilder.AddColumn<string>(
                name: "modified_by_user_id",
                table: "file_metadata",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "",
                comment: "Идентификатор на потребилет променил последно записа");
        }
    }
}
