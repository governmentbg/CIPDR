using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.RegistersCatalog.Migrations
{
    /// <inheritdoc />
    public partial class RegisterStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "registers",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "register_administrations",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "master_person_records",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "administration_person",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "administration",
                type: "timestamptz",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.CreateTable(
                name: "register_file_metadata",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на файла в хранилището"),
                    file_source_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на роля на файла"),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Име на файла"),
                    signature = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Подпис"),
                    hashing_algorithm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Алгоритъм за изчисляване на хеш сума"),
                    hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "Хеш сума"),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на сорс"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_register_file_metadata", x => x.id);
                },
                comment: "Инфорация за качен от потребител файл");

            migrationBuilder.CreateTable(
                name: "register_status",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    register_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на регистър"),
                    status_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на статус"),
                    remark = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Забележка"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_register_status", x => x.id);
                    table.ForeignKey(
                        name: "fk_register_status_registers_register_id",
                        column: x => x.register_id,
                        principalTable: "registers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Статуси на регистър");

            migrationBuilder.CreateIndex(
                name: "ix_register_status_register_id",
                table: "register_status",
                column: "register_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "register_file_metadata");

            migrationBuilder.DropTable(
                name: "register_status");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "registers",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "register_administrations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "master_person_records",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "administration_person",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");

            migrationBuilder.AlterColumn<DateTime>(
                name: "deleted_on",
                table: "administration",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Дата на изтриване",
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true,
                oldComment: "Дата на изтриване");
        }
    }
}
