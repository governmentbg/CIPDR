using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class EDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "e_delivery_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    message_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на съобщение"),
                    message_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Вид съобщение"),
                    step_id = table.Column<int>(type: "integer", nullable: false, comment: "Стъпка"),
                    status_id = table.Column<int>(type: "integer", nullable: false, comment: "Статус"),
                    error_message = table.Column<string>(type: "text", nullable: true, comment: "Грешка"),
                    process_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на заявена услуга, по-която е качен файла"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на администрация"),
                    administration_uic = table.Column<string>(type: "text", nullable: true, comment: "Идентификатор на администрация"),
                    register_id = table.Column<int>(type: "integer", nullable: true, comment: "Идентификатор на регистър"),
                    service_id = table.Column<int>(type: "integer", nullable: true, comment: "Идентификатор на услуга"),
                    message = table.Column<string>(type: "jsonb", nullable: true, comment: "Информация от съобщението при open"),
                    application_json = table.Column<string>(type: "jsonb", nullable: true, comment: "Информация от пдф"),
                    application_submission = table.Column<string>(type: "jsonb", nullable: true, comment: "Информация от пдф json_submission"),
                    rnu = table.Column<string>(type: "text", nullable: true, comment: "Референтен номер на услуга (РНУ)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_e_delivery_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "e_delivery_file_metadata",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    e_delivery_message_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на съобщение"),
                    file_source_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на роля на файла"),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Име на файла"),
                    file_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на файла в хранилището"),
                    blob_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на файл от  съобщение"),
                    rnu = table.Column<string>(type: "text", nullable: true, comment: "Референтен номер на услуга (РНУ)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_e_delivery_file_metadata", x => x.id);
                    table.ForeignKey(
                        name: "fk_e_delivery_file_metadata_e_delivery_messages_e_delivery_mes",
                        column: x => x.e_delivery_message_id,
                        principalTable: "e_delivery_messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Инфорация за качен от потребител файл");

            migrationBuilder.CreateIndex(
                name: "ix_e_delivery_file_metadata_e_delivery_message_id",
                table: "e_delivery_file_metadata",
                column: "e_delivery_message_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "e_delivery_file_metadata");

            migrationBuilder.DropTable(
                name: "e_delivery_messages");
        }
    }
}
