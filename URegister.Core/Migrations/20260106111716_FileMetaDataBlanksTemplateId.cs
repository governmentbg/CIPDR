using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class FileMetaDataBlanksTemplateId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "blank_signature_id",
                table: "file_metadata",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на подписване",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на бланка");

            migrationBuilder.AddColumn<int>(
                name: "blanks_template_id",
                table: "file_metadata",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на бланка");

            migrationBuilder.CreateIndex(
                name: "ix_file_metadata_blanks_template_id",
                table: "file_metadata",
                column: "blanks_template_id");

            migrationBuilder.AddForeignKey(
                name: "fk_file_metadata_blanks_templates_blanks_template_id",
                table: "file_metadata",
                column: "blanks_template_id",
                principalTable: "blanks_templates",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_file_metadata_blanks_templates_blanks_template_id",
                table: "file_metadata");

            migrationBuilder.DropIndex(
                name: "ix_file_metadata_blanks_template_id",
                table: "file_metadata");

            migrationBuilder.DropColumn(
                name: "blanks_template_id",
                table: "file_metadata");

            migrationBuilder.AlterColumn<int>(
                name: "blank_signature_id",
                table: "file_metadata",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на бланка",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Идентификатор на подписване");
        }
    }
}
