using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RegisterAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_administration_person_administrations_administration_id",
                table: "administration_person");

            migrationBuilder.DropIndex(
                name: "ix_administrations_register_id",
                table: "administrations");

            migrationBuilder.DropColumn(
                name: "name",
                table: "administrations");

            migrationBuilder.DropColumn(
                name: "uic",
                table: "administrations");

            migrationBuilder.AddColumn<Guid>(
                name: "administration_id",
                table: "administrations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Идентификатор");

            migrationBuilder.AddColumn<int>(
                name: "register_id",
                table: "administration_person",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Идентификатор на регистър");

            migrationBuilder.CreateTable(
                name: "administration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    uic = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, comment: "ЕИК"),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Име"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата на създаване"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на изтриване")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_administration", x => x.id);
                },
                comment: "Администрации");

            migrationBuilder.CreateIndex(
                name: "ix_administrations_administration_id",
                table: "administrations",
                column: "administration_id");

            migrationBuilder.CreateIndex(
                name: "ix_administrations_register_id_administration_id",
                table: "administrations",
                columns: new[] { "register_id", "administration_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_administration_person_register_id",
                table: "administration_person",
                column: "register_id");

            migrationBuilder.CreateIndex(
                name: "ix_administration_uic",
                table: "administration",
                column: "uic",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_administration_person_administration_administration_id",
                table: "administration_person",
                column: "administration_id",
                principalTable: "administration",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_administration_person_registers_register_id",
                table: "administration_person",
                column: "register_id",
                principalTable: "registers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_administrations_administration_administration_id",
                table: "administrations",
                column: "administration_id",
                principalTable: "administration",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_administration_person_administration_administration_id",
                table: "administration_person");

            migrationBuilder.DropForeignKey(
                name: "fk_administration_person_registers_register_id",
                table: "administration_person");

            migrationBuilder.DropForeignKey(
                name: "fk_administrations_administration_administration_id",
                table: "administrations");

            migrationBuilder.DropTable(
                name: "administration");

            migrationBuilder.DropIndex(
                name: "ix_administrations_administration_id",
                table: "administrations");

            migrationBuilder.DropIndex(
                name: "ix_administrations_register_id_administration_id",
                table: "administrations");

            migrationBuilder.DropIndex(
                name: "ix_administration_person_register_id",
                table: "administration_person");

            migrationBuilder.DropColumn(
                name: "administration_id",
                table: "administrations");

            migrationBuilder.DropColumn(
                name: "register_id",
                table: "administration_person");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "administrations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                comment: "Име");

            migrationBuilder.AddColumn<string>(
                name: "uic",
                table: "administrations",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "",
                comment: "ЕИК");

            migrationBuilder.CreateIndex(
                name: "ix_administrations_register_id",
                table: "administrations",
                column: "register_id");

            migrationBuilder.AddForeignKey(
                name: "fk_administration_person_administrations_administration_id",
                table: "administration_person",
                column: "administration_id",
                principalTable: "administrations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
