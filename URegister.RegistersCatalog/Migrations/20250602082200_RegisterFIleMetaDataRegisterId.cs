using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RegisterFIleMetaDataRegisterId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "source_id",
                table: "register_file_metadata",
                type: "text",
                nullable: true,
                comment: "Идентификатор на сорс",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Идентификатор на сорс");

            migrationBuilder.AddColumn<int>(
                name: "register_id",
                table: "register_file_metadata",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "register_id",
                table: "register_file_metadata");

            migrationBuilder.AlterColumn<Guid>(
                name: "source_id",
                table: "register_file_metadata",
                type: "uuid",
                nullable: true,
                comment: "Идентификатор на сорс",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Идентификатор на сорс");
        }
    }
}
