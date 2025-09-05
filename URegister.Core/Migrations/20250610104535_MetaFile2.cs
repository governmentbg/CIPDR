using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class MetaFile2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "file_source_type_id",
                table: "file_metadata",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на  източник",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на роля на файла");

            migrationBuilder.AddColumn<string>(
                name: "source_id",
                table: "file_metadata",
                type: "text",
                nullable: true,
                comment: "Идентификатор на източник");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source_id",
                table: "file_metadata");

            migrationBuilder.AlterColumn<int>(
                name: "file_source_type_id",
                table: "file_metadata",
                type: "integer",
                nullable: false,
                comment: "Идентификатор на роля на файла",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Идентификатор на  източник");
        }
    }
}
