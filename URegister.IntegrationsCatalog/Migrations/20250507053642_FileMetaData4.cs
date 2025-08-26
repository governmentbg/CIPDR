using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class FileMetaData4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "file_id",
                table: "e_delivery_file_metadata",
                type: "uuid",
                nullable: true,
                comment: "Идентификатор на файла в хранилището",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Идентификатор на файла в хранилището");

            migrationBuilder.AddColumn<string>(
                name: "administration_uic",
                table: "e_delivery_file_metadata",
                type: "text",
                nullable: true,
                comment: "Идентификатор на администрация");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "administration_uic",
                table: "e_delivery_file_metadata");

            migrationBuilder.AlterColumn<Guid>(
                name: "file_id",
                table: "e_delivery_file_metadata",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор на файла в хранилището",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Идентификатор на файла в хранилището");
        }
    }
}
