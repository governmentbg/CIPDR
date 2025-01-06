using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AdministrationPersonAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_administrations_code",
                table: "administrations");

            migrationBuilder.DropColumn(
                name: "code",
                table: "administrations");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "registers",
                type: "boolean",
                nullable: false,
                comment: "Дали записът е активен",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Дали е активен");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "master_person_records",
                type: "boolean",
                nullable: false,
                comment: "Дали записът е активен",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Дали е активен");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "administrations",
                type: "boolean",
                nullable: false,
                comment: "Дали записът е активен",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Дали е активен");

            migrationBuilder.CreateTable(
                name: "administration_person",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    administration_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на администрация"),
                    type = table.Column<int>(type: "integer", nullable: false, comment: "Тип лице"),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Име"),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Презиме"),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Фамилия"),
                    position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Длъжност"),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Телефон"),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Имейл"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата на създаване"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на изтриване")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_administration_person", x => x.id);
                    table.ForeignKey(
                        name: "fk_administration_person_administrations_administration_id",
                        column: x => x.administration_id,
                        principalTable: "administrations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Лица от администрацията");

            migrationBuilder.CreateIndex(
                name: "ix_administration_person_administration_id",
                table: "administration_person",
                column: "administration_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administration_person");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "registers",
                type: "boolean",
                nullable: false,
                comment: "Дали е активен",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Дали записът е активен");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "master_person_records",
                type: "boolean",
                nullable: false,
                comment: "Дали е активен",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Дали записът е активен");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "administrations",
                type: "boolean",
                nullable: false,
                comment: "Дали е активен",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Дали записът е активен");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "administrations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                comment: "ЕБК Код");

            migrationBuilder.CreateIndex(
                name: "ix_administrations_code",
                table: "administrations",
                column: "code");
        }
    }
}
