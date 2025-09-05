using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URegister.Core.Migrations
{
    /// <inheritdoc />
    public partial class ProcessDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "process_delivery",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор"),
                    source_type_id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор на  източник"),
                    source_id = table.Column<string>(type: "text", nullable: true, comment: "Идентификатор на източник"),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на заявена услуга"),
                    channel_id = table.Column<string>(type: "text", nullable: true, comment: "Начин на връчване"),
                    Oписание = table.Column<string>(type: "text", nullable: true),
                    delivery_date = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на връчване"),
                    status_id = table.Column<int>(type: "integer", nullable: false, comment: "Статус"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, comment: "Дали записът е активен"),
                    deleted_on = table.Column<DateTime>(type: "timestamptz", nullable: true, comment: "Дата на изтриване"),
                    modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор на потребителят променил последно записа"),
                    modified_on = table.Column<DateTime>(type: "timestamptz", nullable: false, comment: "Дата на последна промяна")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_delivery", x => x.id);
                    table.ForeignKey(
                        name: "fk_process_delivery_processes_process_id",
                        column: x => x.process_id,
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Информация за връчяане откази/удостоверения/указания");

            migrationBuilder.CreateIndex(
                name: "ix_process_delivery_process_id",
                table: "process_delivery",
                column: "process_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "process_delivery");
        }
    }
}
