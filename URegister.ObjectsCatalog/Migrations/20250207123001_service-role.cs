using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.ObjectsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class servicerole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "steps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "Тип на обработчик на стъпка",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "Тип на обработчик на стъпка");

            migrationBuilder.AlterColumn<string>(
                name: "method",
                table: "steps",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "Метод на обработчик на стъпка",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "Метод на обработчик на стъпка");

            migrationBuilder.AddColumn<Guid>(
                name: "role_id",
                table: "steps",
                type: "uuid",
                nullable: true,
                comment: "Идентификатор на роля");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role_id",
                table: "steps");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "steps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                comment: "Тип на обработчик на стъпка",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true,
                oldComment: "Тип на обработчик на стъпка");

            migrationBuilder.AlterColumn<string>(
                name: "method",
                table: "steps",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                comment: "Метод на обработчик на стъпка",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "Метод на обработчик на стъпка");
        }
    }
}
