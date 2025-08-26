using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "preferred_result_delivery_method",
                table: "processes",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true,
                comment: "Начини на предоставяне на резултата",
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11,
                oldNullable: true,
                oldComment: "Начин на предоставяне на резултата от ЕАУ от номенклатура Начини на предоставяне на резултата от ЕАУ");

            migrationBuilder.AddColumn<DateTime>(
                name: "deadline_date",
                table: "processes",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Срок за изпълнение на услуга");

            migrationBuilder.AddColumn<int>(
                name: "deadline_day",
                table: "processes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Срок за изпълнение на услуга/дни");

            migrationBuilder.AddColumn<int>(
                name: "deadline_id",
                table: "processes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Вид срок за изпълнение на услуга");

            migrationBuilder.AddColumn<string>(
                name: "received_channel_id",
                table: "processes",
                type: "text",
                nullable: true,
                comment: "Начин на получаване на заявлението");

            migrationBuilder.AddColumn<string>(
                name: "Oписание",
                table: "file_metadata",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deadline_date",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "deadline_day",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "deadline_id",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "received_channel_id",
                table: "processes");

            migrationBuilder.DropColumn(
                name: "Oписание",
                table: "file_metadata");

            migrationBuilder.AlterColumn<string>(
                name: "preferred_result_delivery_method",
                table: "processes",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true,
                comment: "Начин на предоставяне на резултата от ЕАУ от номенклатура Начини на предоставяне на резултата от ЕАУ",
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11,
                oldNullable: true,
                oldComment: "Начини на предоставяне на резултата");
        }
    }
}
