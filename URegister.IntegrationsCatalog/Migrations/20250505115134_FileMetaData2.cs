using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class FileMetaData2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "tenant_id",
                table: "e_delivery_file_metadata",
                type: "uuid",
                nullable: true,
                comment: "Идентификатор на администрация",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Идентификатор на администрация");

            migrationBuilder.AddColumn<string>(
                name: "application_json",
                table: "e_delivery_file_metadata",
                type: "jsonb",
                nullable: false,
                defaultValue: "",
                comment: "Информация от пдф");

            migrationBuilder.AddColumn<int>(
                name: "blob_id",
                table: "e_delivery_file_metadata",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на файл от  съобщение");

            migrationBuilder.AddColumn<Guid>(
                name: "file_id",
                table: "e_delivery_file_metadata",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на файла в хранилището");

            migrationBuilder.AddColumn<int>(
                name: "message_id",
                table: "e_delivery_file_metadata",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на съобщение");

            migrationBuilder.AddColumn<int>(
                name: "status_id",
                table: "e_delivery_file_metadata",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Статус");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "application_json",
                table: "e_delivery_file_metadata");

            migrationBuilder.DropColumn(
                name: "blob_id",
                table: "e_delivery_file_metadata");

            migrationBuilder.DropColumn(
                name: "file_id",
                table: "e_delivery_file_metadata");

            migrationBuilder.DropColumn(
                name: "message_id",
                table: "e_delivery_file_metadata");

            migrationBuilder.DropColumn(
                name: "status_id",
                table: "e_delivery_file_metadata");

            migrationBuilder.AlterColumn<Guid>(
                name: "tenant_id",
                table: "e_delivery_file_metadata",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на администрация",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Идентификатор на администрация");
        }
    }
}
