using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class AdministrationIdGuid2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "master_person_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    master_person_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на основно лице"),
                    pid = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Идентификатор на лице"),
                    pid_type = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, comment: "Тип на идентификатора"),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, comment: "Име"),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата на създаване"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на изтриване")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_master_person_records", x => x.id);
                },
                comment: "Глобална партида на лице");

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
                    started_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата на стартиране"),
                    master_person_records_index_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на изтриване")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_registers", x => x.id);
                    table.ForeignKey(
                        name: "fk_registers_master_person_records_master_person_records_index",
                        column: x => x.master_person_records_index_id,
                        principalTable: "master_person_records",
                        principalColumn: "id");
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
                name: "register_person_records",
                columns: table => new
                {
                    register_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на регистър"),
                    master_person_record_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на партида")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_register_person_records", x => new { x.register_id, x.master_person_record_id });
                    table.ForeignKey(
                        name: "fk_register_person_records_master_person_records_master_person",
                        column: x => x.master_person_record_id,
                        principalTable: "master_person_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_register_person_records_registers_register_id",
                        column: x => x.register_id,
                        principalTable: "registers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Записи на лица в регистър");

            migrationBuilder.CreateTable(
                name: "administration_person",
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

            migrationBuilder.CreateIndex(
                name: "ix_administrations_register_id",
                table: "administrations",
                column: "register_id");

            migrationBuilder.CreateIndex(
                name: "ix_master_person_records_pid_pid_type",
                table: "master_person_records",
                columns: new[] { "pid", "pid_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_register_person_records_master_person_record_id",
                table: "register_person_records",
                column: "master_person_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_registers_code",
                table: "registers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_registers_master_person_records_index_id",
                table: "registers",
                column: "master_person_records_index_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administration_person");

            migrationBuilder.DropTable(
                name: "register_person_records");

            migrationBuilder.DropTable(
                name: "administrations");

            migrationBuilder.DropTable(
                name: "registers");

            migrationBuilder.DropTable(
                name: "master_person_records");
        }
    }
}
