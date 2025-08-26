using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class FileMetadatTableAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "file_metadata",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на файла в хранилището"),
                    file_source_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на роля на файла"),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Име на файла"),
                    signature = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Подпис"),
                    hashing_algorithm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Алгоритъм за изчисляване на хеш сума"),
                    hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "Хеш сума"),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на заявена услуга, по-която е качен файла"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false, comment: "Идентификатор на потребилет променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_metadata", x => x.id);
                    table.ForeignKey(
                        name: "fk_file_metadata_processes_process_id",
                        column: x => x.process_id,
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Инфорация за качен от потребител файл");

            migrationBuilder.CreateIndex(
                name: "ix_file_metadata_process_id",
                table: "file_metadata",
                column: "process_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "file_metadata");
        }
    }
}
