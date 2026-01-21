using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class OutMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "out_message_id",
                table: "file_metadata",
                type: "uuid",
                nullable: true,
                comment: "Изпратен ли е към интегратион");

            migrationBuilder.CreateTable(
                name: "out_message",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    message_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Вид съобщение"),
                    status_id = table.Column<int>(type: "integer", nullable: false, comment: "Статус"),
                    error_message = table.Column<string>(type: "text", nullable: true, comment: "Грешка"),
                    process_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на заявена услуга, по-която е качен файла"),
                    source_type = table.Column<int>(type: "integer", nullable: false, comment: "Вид връзка"),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на връзка"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на администрация"),
                    register_id = table.Column<int>(type: "integer", nullable: true, comment: "Идентификатор на регистър"),
                    service_id = table.Column<int>(type: "integer", nullable: true, comment: "Идентификатор на услуга"),
                    message_text = table.Column<string>(type: "text", nullable: true, comment: "Текст на съобщението при изпращане"),
                    subject_text = table.Column<string>(type: "text", nullable: true, comment: "Subject на съобщението при изпращане"),
                    error_count_send = table.Column<int>(type: "integer", nullable: false, comment: "Брой повторения при грешно изпращане"),
                    pid = table.Column<string>(type: "text", nullable: true, comment: "Идентификатор на получател")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_out_message", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_file_metadata_out_message_id",
                table: "file_metadata",
                column: "out_message_id");

            migrationBuilder.AddForeignKey(
                name: "fk_file_metadata_out_message_out_message_id",
                table: "file_metadata",
                column: "out_message_id",
                principalTable: "out_message",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_file_metadata_out_message_out_message_id",
                table: "file_metadata");

            migrationBuilder.DropTable(
                name: "out_message");

            migrationBuilder.DropIndex(
                name: "ix_file_metadata_out_message_id",
                table: "file_metadata");

            migrationBuilder.DropColumn(
                name: "out_message_id",
                table: "file_metadata");
        }
    }
}
