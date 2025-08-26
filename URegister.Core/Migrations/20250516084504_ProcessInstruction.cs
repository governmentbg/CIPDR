using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessInstruction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "instructions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на процес"),
                    content = table.Column<string>(type: "text", nullable: false, comment: "Съдържание"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_instructions", x => x.id);
                    table.ForeignKey(
                        name: "fk_instructions_processes_process_id",
                        column: x => x.process_id,
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Указания");

            migrationBuilder.CreateTable(
                name: "integration_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    source_type = table.Column<int>(type: "integer", nullable: false, comment: "Вид връзка"),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на връзка"),
                    file_name = table.Column<string>(type: "text", nullable: true, comment: "Име на файл"),
                    integration_file_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Име на файл"),
                    file_metadata_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integration_files", x => x.id);
                },
                comment: "Файлове от ССЕВ");

            migrationBuilder.CreateTable(
                name: "instruction_responses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    instruction_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на процес"),
                    content = table.Column<string>(type: "text", nullable: true, comment: "Съдържание"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_instruction_responses", x => x.id);
                    table.ForeignKey(
                        name: "fk_instruction_responses_instructions_instruction_id",
                        column: x => x.instruction_id,
                        principalTable: "instructions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Отговори на Указания");

            migrationBuilder.CreateIndex(
                name: "ix_instruction_responses_instruction_id",
                table: "instruction_responses",
                column: "instruction_id");

            migrationBuilder.CreateIndex(
                name: "ix_instructions_process_id",
                table: "instructions",
                column: "process_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "instruction_responses");

            migrationBuilder.DropTable(
                name: "integration_files");

            migrationBuilder.DropTable(
                name: "instructions");
        }
    }
}
