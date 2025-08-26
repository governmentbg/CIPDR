using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.IntegrationsCatalog.Migrations
{
    /// <inheritdoc />
    public partial class removeFileMetaData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "e_delivery_file_metadata");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "e_delivery_file_metadata",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    administration_uic = table.Column<string>(type: "text", nullable: true, comment: "Идентификатор на администрация"),
                    application_json = table.Column<string>(type: "jsonb", nullable: true, comment: "Информация от пдф"),
                    application_submission = table.Column<string>(type: "jsonb", nullable: true, comment: "Информация от пдф json_submission"),
                    blob_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на файл от  съобщение"),
                    file_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на файла в хранилището"),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Име на файла"),
                    file_source_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на роля на файла"),
                    message_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на съобщение"),
                    process_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на заявена услуга, по-която е качен файла"),
                    register_id = table.Column<int>(type: "integer", nullable: true, comment: "Идентификатор на регистър"),
                    rnu = table.Column<string>(type: "text", nullable: true, comment: "Референтен номер на услуга (РНУ)"),
                    service_id = table.Column<int>(type: "integer", nullable: true, comment: "Идентификатор на услуга"),
                    status_id = table.Column<int>(type: "integer", nullable: false, comment: "Статус"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Идентификатор на администрация")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_e_delivery_file_metadata", x => x.id);
                },
                comment: "Инфорация за качен от потребител файл");
        }
    }
}
