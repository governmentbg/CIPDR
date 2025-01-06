using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class AdministrationIdGuid3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "registers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Код на регистър"),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Име на регистър"),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Описание"),
                    legal_basis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Правно основание"),
                    type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, comment: "Вид на регистъра"),
                    identity_security_level = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, comment: "Ниво на осигуреност на средствата за електронна идентификация"),
                    type_entry = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, comment: "Начин на вписване"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата на създаване"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на изтриване")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_registers", x => x.id);
                },
                comment: "Регистри");

            migrationBuilder.CreateTable(
                name: "administrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    register_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на регистър"),
                    uic = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, comment: "ЕИК"),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Име"),
                    legal_basis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Правно основание"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата на създаване"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на изтриване")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_administrations", x => x.id);
                    table.ForeignKey(
                        name: "fk_administrations_registers_register_id",
                        column: x => x.register_id,
                        principalTable: "registers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Администрации");

            migrationBuilder.CreateTable(
                name: "administration_persons",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    administration_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на администрация"),
                    type = table.Column<string>(type: "text", nullable: false, comment: "Тип лице"),
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
                    table.PrimaryKey("pk_administration_persons", x => x.id);
                    table.ForeignKey(
                        name: "fk_administration_persons_administrations_administration_id",
                        column: x => x.administration_id,
                        principalTable: "administrations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Лица от администрацията");

            migrationBuilder.CreateIndex(
                name: "ix_administration_persons_administration_id",
                table: "administration_persons",
                column: "administration_id");

            migrationBuilder.CreateIndex(
                name: "ix_administrations_register_id",
                table: "administrations",
                column: "register_id");

            migrationBuilder.CreateIndex(
                name: "ix_registers_code",
                table: "registers",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administration_persons");

            migrationBuilder.DropTable(
                name: "administrations");

            migrationBuilder.DropTable(
                name: "registers");
        }
    }
}
