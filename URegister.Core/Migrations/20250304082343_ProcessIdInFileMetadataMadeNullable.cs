using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessIdInFileMetadataMadeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_file_metadata_processes_process_id",
                table: "file_metadata");

            migrationBuilder.AlterColumn<Guid>(
                name: "process_id",
                table: "file_metadata",
                type: "uuid",
                nullable: true,
                comment: "Идентификатор на заявена услуга, по-която е качен файла",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Идентификатор на заявена услуга, по-която е качен файла");

            migrationBuilder.AddForeignKey(
                name: "fk_file_metadata_processes_process_id",
                table: "file_metadata",
                column: "process_id",
                principalTable: "processes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_file_metadata_processes_process_id",
                table: "file_metadata");

            migrationBuilder.AlterColumn<Guid>(
                name: "process_id",
                table: "file_metadata",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на заявена услуга, по-която е качен файла",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Идентификатор на заявена услуга, по-която е качен файла");

            migrationBuilder.AddForeignKey(
                name: "fk_file_metadata_processes_process_id",
                table: "file_metadata",
                column: "process_id",
                principalTable: "processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
