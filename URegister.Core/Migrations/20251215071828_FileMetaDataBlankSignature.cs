using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class FileMetaDataBlankSignature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Oписание",
                table: "file_metadata",
                newName: "description");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "file_metadata",
                type: "text",
                nullable: true,
                comment: "Oписание",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "blank_signature_id",
                table: "file_metadata",
                type: "integer",
                nullable: true,
                comment: "Идентификатор на бланка");

            migrationBuilder.AddColumn<bool>(
                name: "is_stamped",
                table: "file_metadata",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Подпечатан");

            migrationBuilder.AddColumn<Guid>(
                name: "sign_by_id",
                table: "file_metadata",
                type: "uuid",
                nullable: true,
                comment: "Подписан от");

            migrationBuilder.AddColumn<Guid>(
                name: "sign_by_role_id",
                table: "file_metadata",
                type: "uuid",
                nullable: true,
                comment: "Подписан от роля");

            migrationBuilder.AddColumn<int>(
                name: "sign_order",
                table: "file_metadata",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Поредност на подписанване");

            migrationBuilder.AddColumn<bool>(
                name: "has_stamp",
                table: "blanks_templates",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Подпечатва ли се бланката");

            migrationBuilder.CreateIndex(
                name: "ix_file_metadata_blank_signature_id",
                table: "file_metadata",
                column: "blank_signature_id");

            migrationBuilder.AddForeignKey(
                name: "fk_file_metadata_blank_signature_blank_signature_id",
                table: "file_metadata",
                column: "blank_signature_id",
                principalTable: "blank_signature",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_file_metadata_blank_signature_blank_signature_id",
                table: "file_metadata");

            migrationBuilder.DropIndex(
                name: "ix_file_metadata_blank_signature_id",
                table: "file_metadata");

            migrationBuilder.DropColumn(
                name: "blank_signature_id",
                table: "file_metadata");

            migrationBuilder.DropColumn(
                name: "is_stamped",
                table: "file_metadata");

            migrationBuilder.DropColumn(
                name: "sign_by_id",
                table: "file_metadata");

            migrationBuilder.DropColumn(
                name: "sign_by_role_id",
                table: "file_metadata");

            migrationBuilder.DropColumn(
                name: "sign_order",
                table: "file_metadata");

            migrationBuilder.DropColumn(
                name: "has_stamp",
                table: "blanks_templates");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "file_metadata",
                newName: "Oписание");

            migrationBuilder.AlterColumn<string>(
                name: "Oписание",
                table: "file_metadata",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Oписание");
        }
    }
}
